using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class ProfesionalesService : IProfesionalesService
{
    // Entidad de auditoría propia (distinta de "Clinico", que usa Enfermería):
    // así el historial de terapias no se mezcla con el de medicación/patologías
    // en la ficha de otro rol, y cada profesional ve solo lo suyo (se filtra
    // además por EmpleadoId al leer).
    private const string EntidadTerapia = "Terapia";

    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public ProfesionalesService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    public async Task<List<Residente>> ObtenerResidentesAsync()
    {
        return await _db.Residentes
            .Include(r => r.Habitacion)
            .Where(r => r.Estado == EstadoResidente.Activo)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<Residente?> ObtenerResidenteAsync(int residenteId) => await _db.Residentes.FindAsync(residenteId);

    public async Task<List<SesionTerapia>> ObtenerSesionesAsync(int residenteId, int empleadoId)
    {
        return await _db.SesionesTerapia
            .Where(s => s.ResidenteId == residenteId && s.EmpleadoId == empleadoId)
            .OrderByDescending(s => s.FechaHora)
            .ToListAsync();
    }

    public async Task<List<SesionTerapia>> ObtenerAgendaAsync(int empleadoId)
    {
        return await _db.SesionesTerapia
            .Include(s => s.Residente)
            .Where(s => s.EmpleadoId == empleadoId && s.Estado == EstadoSesion.Programada)
            .OrderBy(s => s.FechaHora)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> ObtenerHistorialAsync(int residenteId, int empleadoId)
    {
        var historial = await _db.AuditLogs
            .Where(a => a.EntidadTipo == EntidadTerapia && a.EntidadId == residenteId)
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();

        return historial.Where(a => a.EmpleadoId == empleadoId).ToList();
    }

    public async Task ProgramarSesionAsync(int residenteId, int empleadoId, DateTime fechaHora)
    {
        // El <input type="datetime-local"> del formulario llega sin zona horaria
        // (Kind=Unspecified) y Npgsql exige Utc para "timestamp with time zone".
        // Este MVP es de un solo centro sin gestión de zonas horarias por
        // usuario (igual que el resto de fechas de la app, todas en UTC sin
        // conversión real) — se marca tal cual como Utc, no se convierte.
        var fechaHoraUtc = DateTime.SpecifyKind(fechaHora, DateTimeKind.Utc);

        _db.SesionesTerapia.Add(new SesionTerapia
        {
            CentroId = _tenant.CentroId,
            ResidenteId = residenteId,
            EmpleadoId = empleadoId,
            FechaHora = fechaHoraUtc,
            Estado = EstadoSesion.Programada
        });
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "SesionProgramada", EntidadTerapia, residenteId,
            $"Sesión programada para {fechaHora:dd/MM/yyyy HH:mm}.");
    }

    public async Task MarcarRealizadaAsync(int sesionId, int empleadoId, string? observaciones)
    {
        var sesion = await ObtenerSesionPropiaAsync(sesionId, empleadoId);

        sesion.Estado = EstadoSesion.Realizada;
        sesion.Observaciones = observaciones;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "SesionRealizada", EntidadTerapia, sesion.ResidenteId,
            $"Sesión realizada ({sesion.FechaHora:dd/MM/yyyy HH:mm}).");
    }

    public async Task CancelarSesionAsync(int sesionId, int empleadoId)
    {
        var sesion = await ObtenerSesionPropiaAsync(sesionId, empleadoId);

        sesion.Estado = EstadoSesion.Cancelada;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "SesionCancelada", EntidadTerapia, sesion.ResidenteId,
            $"Sesión cancelada ({sesion.FechaHora:dd/MM/yyyy HH:mm}).");
    }

    // Un profesional solo puede modificar sus propias sesiones — no las de otro
    // compañero de la misma especialidad, aunque compartan residente.
    private async Task<SesionTerapia> ObtenerSesionPropiaAsync(int sesionId, int empleadoId)
    {
        var sesion = await _db.SesionesTerapia.FindAsync(sesionId)
            ?? throw new InvalidOperationException("Sesión no encontrada.");

        if (sesion.EmpleadoId != empleadoId)
        {
            throw new InvalidOperationException("Esta sesión pertenece a otro profesional.");
        }

        return sesion;
    }
}
