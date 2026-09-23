using System.Diagnostics;
using Npgsql;

namespace Cuidexa.Web.Services;

// Backup automático diario de PostgreSQL, sin depender de cron/tareas
// programadas externas — coherente con el monolito autocontenido (misma
// razón por la que Docker/orquestación se dejan fuera hasta que el roadmap
// lo decida explícitamente). Comprueba cada hora si ya existe un backup de
// hoy (UTC); si no, y ya ha pasado la hora configurada, lo ejecuta. Este
// diseño es resiliente a reinicios del servicio (nunca duplica el backup
// del día, y si el servicio estuvo caído a la hora exacta, lo recupera en
// la siguiente comprobación horaria).
//
// Desactivado por defecto ("Backup:Habilitado" = false) para no lanzar
// procesos pg_dump en entornos de desarrollo/test sin que se pida
// explícitamente.
public class BackupService : BackgroundService
{
    private readonly IConfiguration _configuracion;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IConfiguration configuracion, ILogger<BackupService> logger)
    {
        _configuracion = configuracion;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuracion.GetValue("Backup:Habilitado", false))
        {
            _logger.LogInformation("Backup automático desactivado (Backup:Habilitado=false).");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        do
        {
            try
            {
                await ComprobarYEjecutarAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Un fallo de backup no debe tumbar el servicio — se registra
                // como error (visible para alertas) y se reintenta en la
                // siguiente comprobación horaria.
                _logger.LogError(ex, "Fallo al comprobar/ejecutar el backup automático.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ComprobarYEjecutarAsync(CancellationToken ct)
    {
        var directorio = _configuracion.GetValue("Backup:Directorio", "backups")!;
        Directory.CreateDirectory(directorio);

        var hoy = DateTime.UtcNow.Date;
        var prefijoHoy = $"cuidexa-{hoy:yyyyMMdd}";
        if (Directory.EnumerateFiles(directorio, $"{prefijoHoy}*.dump.enc").Any())
        {
            return; // Ya hay backup de hoy.
        }

        var horaObjetivo = _configuracion.GetValue("Backup:HoraUtc", 2);
        if (DateTime.UtcNow.Hour < horaObjetivo)
        {
            return; // Todavía no toca hoy.
        }

        await EjecutarBackupAsync(directorio, ct);
        PurgarAntiguos(directorio);
    }

    private async Task EjecutarBackupAsync(string directorio, CancellationToken ct)
    {
        var claveBase64 = _configuracion["Backup:ClaveCifrado"];
        if (string.IsNullOrWhiteSpace(claveBase64))
        {
            _logger.LogError("Backup automático habilitado pero falta 'Backup:ClaveCifrado' — no se puede cifrar el backup, se omite.");
            return;
        }

        var cadenaConexion = _configuracion.GetConnectionString("Default")!;
        var builder = new NpgsqlConnectionStringBuilder(cadenaConexion);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var rutaTemporal = Path.Combine(directorio, $"cuidexa-{timestamp}.dump.tmp");
        var rutaFinal = Path.Combine(directorio, $"cuidexa-{timestamp}.dump.enc");

        var pgDumpPath = _configuracion.GetValue("Backup:PgDumpPath", "pg_dump")!;
        var proceso = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {builder.Host} -p {builder.Port} -U {builder.Username} -d {builder.Database} -Fc -f \"{rutaTemporal}\"",
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
        // Contraseña por variable de entorno del proceso hijo, nunca en los
        // argumentos de línea de comandos (que quedarían visibles en la
        // lista de procesos del sistema operativo).
        proceso.StartInfo.Environment["PGPASSWORD"] = builder.Password;

        proceso.Start();
        var stderr = await proceso.StandardError.ReadToEndAsync(ct);
        await proceso.WaitForExitAsync(ct);

        if (proceso.ExitCode != 0)
        {
            _logger.LogError("pg_dump falló (código {CodigoSalida}): {Error}", proceso.ExitCode, stderr);
            if (File.Exists(rutaTemporal))
            {
                File.Delete(rutaTemporal);
            }
            return;
        }

        await BackupCifrado.CifrarArchivoAsync(rutaTemporal, rutaFinal, Convert.FromBase64String(claveBase64));
        File.Delete(rutaTemporal);

        var tamanoMb = new FileInfo(rutaFinal).Length / 1024.0 / 1024.0;
        _logger.LogInformation("Backup automático completado: {Archivo} ({TamanoMb:0.00} MB).", rutaFinal, tamanoMb);
    }

    private void PurgarAntiguos(string directorio)
    {
        var retencionDias = _configuracion.GetValue("Backup:RetencionDias", 30);
        var limite = DateTime.UtcNow.Date.AddDays(-retencionDias);

        foreach (var archivo in Directory.EnumerateFiles(directorio, "cuidexa-*.dump.enc"))
        {
            if (File.GetCreationTimeUtc(archivo).Date < limite)
            {
                File.Delete(archivo);
                _logger.LogInformation("Backup antiguo eliminado por retención ({RetencionDias} días): {Archivo}", retencionDias, archivo);
            }
        }
    }
}
