using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class GrupoNotificacionService : IGrupoNotificacionService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly ITenantContext _tenant;

    public GrupoNotificacionService(CuidexaDbContext db, IAuditService auditoria, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _tenant = tenant;
    }

    public async Task<List<GrupoNotificacion>> ObtenerTodosAsync() =>
        await _db.GruposNotificacion.Include(g => g.Centro).OrderBy(g => g.Nombre).ToListAsync();

    public async Task<List<GrupoNotificacion>> ObtenerActivosAsync() =>
        await _db.GruposNotificacion.Where(g => g.Activo).OrderBy(g => g.Nombre).ToListAsync();

    public async Task<GrupoNotificacion?> ObtenerPorIdAsync(int id) => await _db.GruposNotificacion.FindAsync(id);

    public async Task<List<Empleado>> ObtenerMiembrosAsync(int grupoId)
    {
        return await _db.GruposNotificacionEmpleados
            .Where(m => m.GrupoNotificacionId == grupoId)
            .Include(m => m.Empleado)
            .Select(m => m.Empleado!)
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task<GrupoNotificacion> CrearAsync(GrupoNotificacionDto dto, int? empleadoIdActor, int? centroId = null)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El grupo necesita un nombre.");
        }

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

        // El grupo pertenece a un único centro, así que sus miembros tienen
        // que ser de ese mismo centro (un aviso a grupo genera una fila por
        // miembro con el centro del propio grupo, ver EventoDistribucionService).
        var idsUnicos = dto.EmpleadoIds.Distinct().ToList();
        var miembrosFueraDeCentro = await _db.Empleados.IgnoreQueryFilters()
            .Where(e => idsUnicos.Contains(e.Id) && e.CentroId != centroIdEfectivo)
            .AnyAsync();
        if (miembrosFueraDeCentro)
        {
            throw new InvalidOperationException("Todos los miembros deben pertenecer al centro elegido para el grupo.");
        }

        var grupo = new GrupoNotificacion { CentroId = centroIdEfectivo, Nombre = dto.Nombre, Activo = true };
        _db.GruposNotificacion.Add(grupo);
        await _db.SaveChangesAsync();

        foreach (var empleadoId in idsUnicos)
        {
            _db.GruposNotificacionEmpleados.Add(new GrupoNotificacionEmpleado { CentroId = centroIdEfectivo, GrupoNotificacionId = grupo.Id, EmpleadoId = empleadoId });
        }
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, "Alta", "GrupoNotificacion", grupo.Id,
            $"Alta del grupo '{grupo.Nombre}' con {dto.EmpleadoIds.Count} miembro(s).");

        return grupo;
    }

    public async Task ActualizarAsync(int id, GrupoNotificacionDto dto, int? empleadoIdActor)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El grupo necesita un nombre.");
        }

        var grupo = await _db.GruposNotificacion.FindAsync(id)
            ?? throw new InvalidOperationException("Grupo no encontrado.");

        var idsUnicos = dto.EmpleadoIds.Distinct().ToList();
        var miembrosFueraDeCentro = await _db.Empleados.IgnoreQueryFilters()
            .Where(e => idsUnicos.Contains(e.Id) && e.CentroId != grupo.CentroId)
            .AnyAsync();
        if (miembrosFueraDeCentro)
        {
            throw new InvalidOperationException("Todos los miembros deben pertenecer al centro del grupo.");
        }

        grupo.Nombre = dto.Nombre;

        var actuales = await _db.GruposNotificacionEmpleados.Where(m => m.GrupoNotificacionId == id).ToListAsync();
        _db.GruposNotificacionEmpleados.RemoveRange(actuales);
        foreach (var empleadoId in idsUnicos)
        {
            _db.GruposNotificacionEmpleados.Add(new GrupoNotificacionEmpleado { CentroId = grupo.CentroId, GrupoNotificacionId = id, EmpleadoId = empleadoId });
        }

        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, "Modificacion", "GrupoNotificacion", id,
            $"Grupo '{grupo.Nombre}' actualizado a {dto.EmpleadoIds.Count} miembro(s).");
    }

    public async Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor)
    {
        var grupo = await _db.GruposNotificacion.FindAsync(id)
            ?? throw new InvalidOperationException("Grupo no encontrado.");

        grupo.Activo = activo;
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, activo ? "Activacion" : "Desactivacion", "GrupoNotificacion", id,
            activo ? "Grupo reactivado." : "Grupo desactivado.");
    }
}
