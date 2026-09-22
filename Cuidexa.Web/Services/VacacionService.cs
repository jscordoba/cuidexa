using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class VacacionService : IVacacionService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public VacacionService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    public async Task<List<Vacacion>> ObtenerTodasAsync()
    {
        return await _db.Vacaciones
            .Include(v => v.Empleado)
            .OrderByDescending(v => v.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<List<Vacacion>> ObtenerPorEmpleadoAsync(int empleadoId)
    {
        return await _db.Vacaciones
            .Where(v => v.EmpleadoId == empleadoId)
            .OrderByDescending(v => v.FechaInicio)
            .ToListAsync();
    }

    public async Task<List<Vacacion>> ObtenerPendientesAsync()
    {
        return await _db.Vacaciones
            .Include(v => v.Empleado)
            .Where(v => v.Estado == EstadoVacacion.Solicitada)
            .OrderBy(v => v.FechaInicio)
            .ToListAsync();
    }

    public async Task<List<ResumenVacacionesEmpleado>> ObtenerResumenAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var festivos = await ObtenerFechasFeriadosAsync();

        var vacaciones = await _db.Vacaciones.Include(v => v.Empleado).ToListAsync();

        return vacaciones
            .Where(v => v.Empleado is not null)
            .GroupBy(v => v.Empleado!)
            .Select(g => new ResumenVacacionesEmpleado
            {
                EmpleadoId = g.Key.Id,
                Nombre = g.Key.Nombre,
                Rol = g.Key.Rol,
                DiasDisfrutados = g.Where(v => v.Estado == EstadoVacacion.Aprobada && v.FechaFin < hoy)
                    .Sum(v => CalculoDiasVacaciones.Contar(v.FechaInicio, v.FechaFin, v.IncluyeFinesSemanaYFestivos, festivos)),
                DiasSolicitados = g.Where(v => v.Estado == EstadoVacacion.Solicitada)
                    .Sum(v => CalculoDiasVacaciones.Contar(v.FechaInicio, v.FechaFin, v.IncluyeFinesSemanaYFestivos, festivos))
            })
            .OrderBy(r => r.Nombre)
            .ToList();
    }

    public async Task RegistrarAsync(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string? notas, bool incluyeFinesSemanaYFestivos, int? actorId)
    {
        if (fechaFin < fechaInicio)
        {
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la de inicio.");
        }

        // El centro se deriva siempre del propio empleado (no de quien
        // registra) — con DirectorOrganizacion, ese empleado puede ser de
        // cualquier centro de su organización, no necesariamente el suyo.
        var empleadoDestino = await _db.Empleados.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == empleadoId)
            ?? throw new InvalidOperationException("Empleado no encontrado.");

        var vacacion = new Vacacion
        {
            CentroId = empleadoDestino.CentroId,
            EmpleadoId = empleadoId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Notas = notas,
            IncluyeFinesSemanaYFestivos = incluyeFinesSemanaYFestivos,
            Estado = EstadoVacacion.Aprobada,
            ResueltoPorId = actorId,
            FechaResolucion = DateTime.UtcNow
        };

        _db.Vacaciones.Add(vacacion);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "VacacionRegistrada", "Vacacion", vacacion.Id,
            $"Vacaciones de Empleado #{empleadoId} del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}.");
    }

    public async Task SolicitarAsync(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string? notas)
    {
        if (fechaFin < fechaInicio)
        {
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la de inicio.");
        }

        var vacacion = new Vacacion
        {
            CentroId = _tenant.CentroId,
            EmpleadoId = empleadoId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Notas = notas,
            Estado = EstadoVacacion.Solicitada
        };

        _db.Vacaciones.Add(vacacion);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "VacacionSolicitada", "Vacacion", vacacion.Id,
            $"Vacaciones solicitadas del {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}.");
    }

    public async Task AprobarAsync(int vacacionId, bool incluyeFinesSemanaYFestivos, int? actorId)
    {
        var vacacion = await _db.Vacaciones.FindAsync(vacacionId)
            ?? throw new InvalidOperationException("Solicitud de vacaciones no encontrada.");

        vacacion.Estado = EstadoVacacion.Aprobada;
        vacacion.IncluyeFinesSemanaYFestivos = incluyeFinesSemanaYFestivos;
        vacacion.ResueltoPorId = actorId;
        vacacion.FechaResolucion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "VacacionAprobada", "Vacacion", vacacion.Id, "Vacaciones aprobadas.");
    }

    public async Task RechazarAsync(int vacacionId, int? actorId)
    {
        var vacacion = await _db.Vacaciones.FindAsync(vacacionId)
            ?? throw new InvalidOperationException("Solicitud de vacaciones no encontrada.");

        vacacion.Estado = EstadoVacacion.Rechazada;
        vacacion.ResueltoPorId = actorId;
        vacacion.FechaResolucion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "VacacionRechazada", "Vacacion", vacacion.Id, "Vacaciones rechazadas.");
    }

    public async Task<List<Feriado>> ObtenerFeriadosAsync()
    {
        return await _db.Feriados.OrderBy(f => f.Fecha).ToListAsync();
    }

    public async Task<HashSet<DateOnly>> ObtenerFechasFeriadosAsync()
    {
        return (await _db.Feriados.Select(f => f.Fecha).ToListAsync()).ToHashSet();
    }

    public async Task CrearFeriadoAsync(DateOnly fecha, string? nombre)
    {
        if (await _db.Feriados.AnyAsync(f => f.Fecha == fecha))
        {
            throw new InvalidOperationException("Ya existe un festivo registrado en esa fecha.");
        }

        _db.Feriados.Add(new Feriado { CentroId = _tenant.CentroId, Fecha = fecha, Nombre = nombre });
        await _db.SaveChangesAsync();
    }

    public async Task EliminarFeriadoAsync(int id)
    {
        var feriado = await _db.Feriados.FindAsync(id);
        if (feriado is not null)
        {
            _db.Feriados.Remove(feriado);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<(int Importados, int Omitidos)> ImportarFeriadosAsync(List<(DateOnly Fecha, string Nombre)> festivos, int? actorId)
    {
        var existentes = (await _db.Feriados.Select(f => f.Fecha).ToListAsync()).ToHashSet();
        var importados = 0;
        var omitidos = 0;

        foreach (var (fecha, nombre) in festivos)
        {
            if (!existentes.Add(fecha))
            {
                omitidos++;
                continue;
            }

            _db.Feriados.Add(new Feriado { CentroId = _tenant.CentroId, Fecha = fecha, Nombre = nombre });
            importados++;
        }

        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(actorId, "FestivosImportados", "Feriado", 0,
            $"Importados {importados} festivos ({omitidos} ya existían).");

        return (importados, omitidos);
    }
}
