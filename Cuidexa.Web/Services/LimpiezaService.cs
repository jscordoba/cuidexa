using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class LimpiezaService : ILimpiezaService
{
    private readonly CuidexaDbContext _db;
    private readonly ITenantContext _tenant;

    public LimpiezaService(CuidexaDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<List<TareaLimpieza>> ObtenerPendientesAsync()
    {
        return await _db.TareasLimpieza
            .Include(t => t.Habitacion)
            .Where(t => t.Estado == EstadoTarea.Pendiente)
            .OrderBy(t => t.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<TareaLimpieza>> ObtenerCompletadasRecientesAsync()
    {
        return await _db.TareasLimpieza
            .Include(t => t.Habitacion)
            .Include(t => t.Empleado)
            .Where(t => t.Estado == EstadoTarea.Completada)
            .OrderByDescending(t => t.FechaCompletada)
            .Take(20)
            .ToListAsync();
    }

    // Tareas de zonas comunes: las inicia el propio equipo de limpieza (no
    // hay un "alta" que las dispare automáticamente, a diferencia de las
    // habitaciones — ver ResidenteService para esas).
    public async Task CrearTareaZonaAsync(string zona, string descripcion)
    {
        _db.TareasLimpieza.Add(new TareaLimpieza
        {
            CentroId = _tenant.CentroId,
            Zona = zona,
            Descripcion = descripcion,
            Estado = EstadoTarea.Pendiente
        });
        await _db.SaveChangesAsync();
    }

    public async Task MarcarCompletadaAsync(int tareaId, int? empleadoId)
    {
        var tarea = await _db.TareasLimpieza.FindAsync(tareaId)
            ?? throw new InvalidOperationException("Tarea no encontrada.");

        tarea.Estado = EstadoTarea.Completada;
        tarea.FechaCompletada = DateTime.UtcNow;
        tarea.EmpleadoId = empleadoId;
        await _db.SaveChangesAsync();
    }
}
