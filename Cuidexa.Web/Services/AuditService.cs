using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class AuditService : IAuditService
{
    private readonly CuidexaDbContext _db;
    private readonly ITenantContext _tenant;

    public AuditService(CuidexaDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task RegistrarAsync(int? empleadoId, string accion, string entidadTipo, int entidadId, string detalle, int? centroIdExplicito = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            CentroId = centroIdExplicito ?? _tenant.CentroId,
            EmpleadoId = empleadoId,
            Accion = accion,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            Detalle = detalle
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> ObtenerPorEntidadAsync(string entidadTipo, int entidadId)
    {
        return await _db.AuditLogs
            .Include(a => a.Empleado)
            .Where(a => a.EntidadTipo == entidadTipo && a.EntidadId == entidadId)
            .OrderByDescending(a => a.FechaHora)
            .ToListAsync();
    }
}
