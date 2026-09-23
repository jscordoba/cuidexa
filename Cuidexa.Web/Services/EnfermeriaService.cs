using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class EnfermeriaService : IEnfermeriaService
{
    // Usamos un EntidadTipo distinto de "Residente" a propósito: así el
    // historial clínico (patologías, medicación) nunca aparece en la vista
    // de auditoría que ve Admin en la ficha del residente — es información
    // que solo debe ver Enfermería.
    private const string EntidadClinica = "Clinico";

    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public EnfermeriaService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    public async Task<List<Residente>> ObtenerResidentesAsync()
    {
        return await _db.Residentes
            .Where(r => r.Estado == EstadoResidente.Activo)
            .Include(r => r.Patologias).ThenInclude(p => p.Patologia)
            .Include(r => r.Medicaciones)
            .AsSplitQuery()
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<Residente?> ObtenerDetalleAsync(int residenteId)
    {
        return await _db.Residentes
            .Include(r => r.Patologias).ThenInclude(p => p.Patologia)
            .Include(r => r.Medicaciones).ThenInclude(m => m.Registros)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == residenteId);
    }

    public async Task<List<AuditLog>> ObtenerHistorialClinicoAsync(int residenteId)
    {
        return await _db.AuditLogs
            .Include(a => a.Empleado)
            .Where(a => a.EntidadTipo == EntidadClinica && a.EntidadId == residenteId)
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();
    }

    public async Task<List<AgendaMedicacionItem>> ObtenerAgendaHoyAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var medicacionesActivas = await _db.Medicaciones
            .Include(m => m.Residente)
            .Include(m => m.Registros)
            .Where(m => m.FechaFin == null && m.Residente!.Estado == EstadoResidente.Activo)
            .OrderBy(m => m.Residente!.Nombre)
            .ToListAsync();

        return medicacionesActivas.Select(m => new AgendaMedicacionItem
        {
            ResidenteId = m.ResidenteId,
            ResidenteNombre = $"{m.Residente!.Nombre} {m.Residente.Apellidos}",
            MedicacionId = m.Id,
            Nombre = m.Nombre,
            Dosis = m.Dosis,
            Horario = m.Horario,
            AdministradaHoy = m.Registros.Any(r => DateOnly.FromDateTime(r.FechaHora) == hoy)
        }).ToList();
    }

    public async Task AgregarPatologiaAsync(int residenteId, int patologiaId, string? observaciones, int? empleadoId)
    {
        var yaExiste = await _db.ResidentePatologias.AnyAsync(rp => rp.ResidenteId == residenteId && rp.PatologiaId == patologiaId);
        if (yaExiste) return;

        _db.ResidentePatologias.Add(new ResidentePatologia
        {
            CentroId = _tenant.CentroId,
            ResidenteId = residenteId,
            PatologiaId = patologiaId,
            Observaciones = observaciones
        });
        await _db.SaveChangesAsync();

        var patologia = await _db.Patologias.FindAsync(patologiaId);
        await _auditoria.RegistrarAsync(empleadoId, "PatologiaAlta", EntidadClinica, residenteId,
            $"Patología añadida: {patologia?.Nombre}.");
    }

    public async Task QuitarPatologiaAsync(int residenteId, int patologiaId, int? empleadoId)
    {
        var relacion = await _db.ResidentePatologias.FirstOrDefaultAsync(rp => rp.ResidenteId == residenteId && rp.PatologiaId == patologiaId);
        if (relacion is null) return;

        _db.ResidentePatologias.Remove(relacion);
        await _db.SaveChangesAsync();

        var patologia = await _db.Patologias.FindAsync(patologiaId);
        await _auditoria.RegistrarAsync(empleadoId, "PatologiaBaja", EntidadClinica, residenteId,
            $"Patología retirada: {patologia?.Nombre}.");
    }

    public async Task AgregarMedicacionAsync(int residenteId, MedicacionCreateDto dto, int? empleadoId)
    {
        _db.Medicaciones.Add(new Medicacion
        {
            CentroId = _tenant.CentroId,
            ResidenteId = residenteId,
            Nombre = dto.Nombre,
            Dosis = dto.Dosis,
            Horario = dto.Horario,
            Instrucciones = dto.Instrucciones,
            FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "MedicacionAlta", EntidadClinica, residenteId,
            $"Medicación añadida: {dto.Nombre} ({dto.Dosis}, {dto.Horario}).");
    }

    public async Task FinalizarMedicacionAsync(int medicacionId, int? empleadoId)
    {
        var medicacion = await _db.Medicaciones.FindAsync(medicacionId)
            ?? throw new InvalidOperationException("Medicación no encontrada.");

        medicacion.FechaFin = DateOnly.FromDateTime(DateTime.UtcNow);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "MedicacionBaja", EntidadClinica, medicacion.ResidenteId,
            $"Medicación finalizada: {medicacion.Nombre}.");
    }

    public async Task RegistrarAdministracionAsync(int medicacionId, string? observaciones, int? empleadoId)
    {
        var medicacion = await _db.Medicaciones.FindAsync(medicacionId)
            ?? throw new InvalidOperationException("Medicación no encontrada.");

        _db.RegistrosAdministracion.Add(new RegistroAdministracion
        {
            CentroId = _tenant.CentroId,
            MedicacionId = medicacionId,
            EmpleadoId = empleadoId,
            Observaciones = observaciones
        });
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoId, "Administracion", EntidadClinica, medicacion.ResidenteId,
            $"Dosis administrada: {medicacion.Nombre}.");
    }
}
