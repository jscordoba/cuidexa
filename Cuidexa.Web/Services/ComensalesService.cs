using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class ComensalesService : IComensalesService
{
    private readonly CuidexaDbContext _db;
    private readonly ITurnoService _turnos;

    public ComensalesService(CuidexaDbContext db, ITurnoService turnos)
    {
        _db = db;
        _turnos = turnos;
    }

    public async Task<ComensalesHoyViewModel> ObtenerComensalesAsync(DateOnly fecha)
    {
        // Todos los residentes activos comen — este MVP no modela ausencias
        // puntuales (ingreso hospitalario, visita familiar...), así que la
        // plantilla activa de hoy es la mejor estimación también para fechas
        // cercanas (ver/planificar el día siguiente, por ejemplo).
        var residentesActivos = await _db.Residentes
            .Where(r => r.Estado == EstadoResidente.Activo)
            .Include(r => r.Habitacion)
            .Include(r => r.Dietas).ThenInclude(d => d.Dieta)
            .OrderBy(r => r.Nombre)
            .ToListAsync();

        var residentesComensales = residentesActivos.Select(r => new ResidenteComensalItem
        {
            Id = r.Id,
            Nombre = $"{r.Nombre} {r.Apellidos}",
            Habitacion = r.Habitacion?.Numero,
            Dieta = r.Dietas.FirstOrDefault(d => d.FechaFin == null)?.Dieta?.Nombre ?? "Sin dieta asignada"
        }).ToList();

        // Turno realmente cubierto ese día por cada empleado (por defecto o
        // por excepción) — un "cedido" no cuenta como personal en turno ese
        // día, así que se descarta antes de construir la lista.
        var turnosDelDia = (await _turnos.ObtenerTurnosEfectivosAsync(fecha, fecha, null))
            .Where(t => !t.Cedido)
            .OrderBy(t => t.HoraInicio)
            .ToList();

        var empleadosComensales = turnosDelDia.Select(t => new EmpleadoComensalItem
        {
            Nombre = t.EmpleadoNombre,
            Rol = t.Rol,
            BloqueHorario = ObtenerBloqueHorario(t.HoraInicio),
            HoraInicio = t.HoraInicio,
            HoraFin = t.HoraFin
        }).ToList();

        return new ComensalesHoyViewModel
        {
            Fecha = fecha,
            Residentes = residentesComensales,
            Empleados = empleadosComensales
        };
    }

    private static string ObtenerBloqueHorario(TimeOnly horaInicio)
    {
        var hora = horaInicio.Hour;
        if (hora is >= 6 and < 14) return "Mañana";
        if (hora is >= 14 and < 21) return "Tarde";
        return "Noche";
    }
}
