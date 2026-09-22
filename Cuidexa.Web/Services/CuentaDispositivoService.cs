using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class CuentaDispositivoService : ICuentaDispositivoService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly IEventoDistribucionService _eventos;
    private readonly ITurnoService _turnos;
    private readonly IComensalesService _comensales;
    private readonly IEnfermeriaService _enfermeria;
    private readonly ITenantContext _tenant;

    // Únicos departamentos con tablet compartida en esta fase — ver plan de
    // Fase 6 (Limpieza/Profesionales quedaron fuera a propósito).
    private static readonly RolEmpleado[] RolesPermitidos =
        { RolEmpleado.Cocina, RolEmpleado.Enfermeria, RolEmpleado.Auxiliar };

    public CuentaDispositivoService(
        CuidexaDbContext db,
        IAuditService auditoria,
        IEventoDistribucionService eventos,
        ITurnoService turnos,
        IComensalesService comensales,
        IEnfermeriaService enfermeria,
        ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _eventos = eventos;
        _turnos = turnos;
        _comensales = comensales;
        _enfermeria = enfermeria;
        _tenant = tenant;
    }

    public async Task<List<CuentaDispositivo>> ObtenerTodasAsync() =>
        await _db.CuentasDispositivo.Include(c => c.Centro).OrderBy(c => c.Nombre).ToListAsync();

    public async Task<CuentaDispositivo?> ObtenerPorIdAsync(int id) => await _db.CuentasDispositivo.FindAsync(id);

    private static void ValidarRol(RolEmpleado rol)
    {
        if (!RolesPermitidos.Contains(rol))
        {
            throw new InvalidOperationException("Solo Cocina, Enfermería o Auxiliares pueden tener una tablet compartida.");
        }
    }

    public async Task<CuentaDispositivo> CrearAsync(CuentaDispositivoCreateDto dto, int? empleadoIdActor, int? centroId = null)
    {
        ValidarRol(dto.Rol);

        int centroIdEfectivo;
        if (centroId.HasValue)
        {
            var perteneceAMiOrganizacion = await _db.Centros.AnyAsync(c => c.Id == centroId.Value && c.OrganizacionId == _tenant.OrganizacionId);
            if (!perteneceAMiOrganizacion)
            {
                throw new InvalidOperationException("El centro elegido no pertenece a tu organización.");
            }
            centroIdEfectivo = centroId.Value;
        }
        else
        {
            centroIdEfectivo = _tenant.CentroId;
        }

        // (CentroId, Nombre) es la unicidad real (Fase 9) — comprobar solo en
        // el centro destino, no en todos los de la organización.
        if (await _db.CuentasDispositivo.IgnoreQueryFilters().AnyAsync(c => c.CentroId == centroIdEfectivo && c.Nombre == dto.Nombre))
        {
            throw new InvalidOperationException("Ya existe una tablet con ese nombre en ese centro.");
        }

        PasswordHasher.Validar(dto.Password);

        var cuenta = new CuentaDispositivo
        {
            CentroId = centroIdEfectivo,
            Nombre = dto.Nombre,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Rol = dto.Rol,
            Activo = true
        };

        _db.CuentasDispositivo.Add(cuenta);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, "Alta", "Dispositivo", cuenta.Id,
            $"Alta de tablet '{cuenta.Nombre}' para {cuenta.Rol}.");

        return cuenta;
    }

    public async Task ActualizarAsync(int id, CuentaDispositivoEditDto dto, int? empleadoIdActor)
    {
        ValidarRol(dto.Rol);

        var cuenta = await _db.CuentasDispositivo.FindAsync(id)
            ?? throw new InvalidOperationException("Tablet no encontrada.");

        if (await _db.CuentasDispositivo.IgnoreQueryFilters().AnyAsync(c => c.CentroId == cuenta.CentroId && c.Nombre == dto.Nombre && c.Id != id))
        {
            throw new InvalidOperationException("Ya existe otra tablet con ese nombre en ese centro.");
        }

        var cambios = new List<string>();
        if (cuenta.Nombre != dto.Nombre) cambios.Add($"Nombre: {cuenta.Nombre} → {dto.Nombre}");
        if (cuenta.Rol != dto.Rol) cambios.Add($"Rol: {cuenta.Rol} → {dto.Rol}");

        cuenta.Nombre = dto.Nombre;
        cuenta.Rol = dto.Rol;

        if (!string.IsNullOrWhiteSpace(dto.NuevaPassword))
        {
            PasswordHasher.Validar(dto.NuevaPassword);
            cuenta.PasswordHash = PasswordHasher.Hash(dto.NuevaPassword);
            var estabaBloqueada = BloqueoCuenta.EstaBloqueada(cuenta);
            BloqueoCuenta.Desbloquear(cuenta);
            cambios.Add(estabaBloqueada ? "Contraseña restablecida (cuenta desbloqueada)" : "Contraseña restablecida");
        }

        await _db.SaveChangesAsync();

        if (cambios.Count > 0)
        {
            await _auditoria.RegistrarAsync(empleadoIdActor, "Modificacion", "Dispositivo", cuenta.Id, string.Join("; ", cambios));
        }
    }

    public async Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor)
    {
        var cuenta = await _db.CuentasDispositivo.FindAsync(id)
            ?? throw new InvalidOperationException("Tablet no encontrada.");

        cuenta.Activo = activo;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, activo ? "Activacion" : "Desactivacion", "Dispositivo", cuenta.Id,
            activo ? "Tablet reactivada." : "Tablet desactivada.");
    }

    // Lanza InvalidOperationException si la cuenta existe pero está bloqueada
    // por fuerza bruta (mensaje distinto de "credenciales incorrectas", para
    // que quien la usa físicamente sepa que tiene que esperar o avisar a un
    // Admin) — devuelve null para "no existe" o "contraseña incorrecta".
    public async Task<CuentaDispositivo?> AutenticarAsync(string nombre, string password, int centroId)
    {
        var cuenta = await _db.CuentasDispositivo.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Nombre == nombre && c.CentroId == centroId && c.Activo);
        if (cuenta is null)
        {
            return null;
        }

        if (BloqueoCuenta.EstaBloqueada(cuenta))
        {
            throw new InvalidOperationException($"Demasiados intentos fallidos. Inténtalo de nuevo en {BloqueoCuenta.MinutosRestantes(cuenta)} minuto(s), o pide a un Admin que restablezca la contraseña de esta tablet.");
        }

        if (!PasswordHasher.Verify(password, cuenta.PasswordHash))
        {
            BloqueoCuenta.RegistrarFallo(cuenta);
            await _db.SaveChangesAsync();
            return null;
        }

        BloqueoCuenta.Desbloquear(cuenta);
        cuenta.UltimoAcceso = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return cuenta;
    }

    public async Task<DispositivoDashboardViewModel> ObtenerResumenAsync(CuentaDispositivo cuenta)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var avisos = await _eventos.ObtenerPorRolAsync(cuenta.Rol);
        var pendientes = avisos.Where(e => !e.Leido).ToList();

        var resumen = new DispositivoDashboardViewModel
        {
            NombreDispositivo = cuenta.Nombre,
            Rol = cuenta.Rol,
            AvisosPendientesCount = pendientes.Count,
            AvisosPendientes = pendientes.Take(8).ToList(),
            PersonalHoy = await _turnos.ObtenerTurnosEfectivosAsync(hoy, hoy, cuenta.Rol)
        };

        if (cuenta.Rol == RolEmpleado.Cocina)
        {
            resumen.ComensalesHoy = await _comensales.ObtenerComensalesAsync(hoy);
        }

        if (cuenta.Rol == RolEmpleado.Enfermeria)
        {
            var agenda = await _enfermeria.ObtenerAgendaHoyAsync();
            resumen.MedicacionPendienteHoy = agenda.Count(a => !a.AdministradaHoy);
        }

        return resumen;
    }
}
