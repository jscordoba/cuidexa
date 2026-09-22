using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class InformesService : IInformesService
{
    private readonly CuidexaDbContext _db;
    private readonly ITurnoService _turnos;
    private readonly IVacacionService _vacaciones;

    public InformesService(CuidexaDbContext db, ITurnoService turnos, IVacacionService vacaciones)
    {
        _db = db;
        _turnos = turnos;
        _vacaciones = vacaciones;
    }

    private static DateTime InicioUtc(DateOnly fecha) => DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    private static DateTime FinUtc(DateOnly fecha) => DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

    private static double HorasTurno(TimeOnly inicio, TimeOnly fin)
    {
        var duracion = fin > inicio ? fin - inicio : (TimeSpan.FromHours(24) - inicio.ToTimeSpan()) + fin.ToTimeSpan();
        return duracion.TotalHours;
    }

    public async Task<InformeTurnosPersonal> ObtenerTurnosPersonalAsync(DateOnly desde, DateOnly hasta)
    {
        var turnos = await _turnos.ObtenerTurnosEfectivosAsync(desde, hasta, null);
        var festivos = await _vacaciones.ObtenerFechasFeriadosAsync();

        var vacacionesSolapadas = await _db.Vacaciones
            .Where(v => v.FechaInicio <= hasta && v.FechaFin >= desde)
            .ToListAsync();

        var filas = turnos
            .Where(t => t.EmpleadoId.HasValue)
            .GroupBy(t => new { t.EmpleadoId, t.EmpleadoNombre, t.Rol })
            .Select(g =>
            {
                var vacacionesEmpleado = vacacionesSolapadas.Where(v => v.EmpleadoId == g.Key.EmpleadoId).ToList();

                return new FilaTurnosPersonal
                {
                    EmpleadoId = g.Key.EmpleadoId!.Value,
                    Nombre = g.Key.EmpleadoNombre,
                    Rol = g.Key.Rol,
                    HorasTrabajadas = g.Where(t => !t.Cedido).Sum(t => HorasTurno(t.HoraInicio, t.HoraFin)),
                    DiasAusencia = g.Count(t => t.Cedido),
                    VacacionesDisfrutadas = vacacionesEmpleado
                        .Where(v => v.Estado == EstadoVacacion.Aprobada)
                        .Sum(v => CalculoDiasVacaciones.Contar(
                            v.FechaInicio > desde ? v.FechaInicio : desde,
                            v.FechaFin < hasta ? v.FechaFin : hasta,
                            v.IncluyeFinesSemanaYFestivos, festivos)),
                    VacacionesSolicitadas = vacacionesEmpleado
                        .Where(v => v.Estado == EstadoVacacion.Solicitada)
                        .Sum(v => CalculoDiasVacaciones.Contar(
                            v.FechaInicio > desde ? v.FechaInicio : desde,
                            v.FechaFin < hasta ? v.FechaFin : hasta,
                            v.IncluyeFinesSemanaYFestivos, festivos))
                };
            })
            .OrderBy(f => f.Nombre)
            .ToList();

        return new InformeTurnosPersonal { Desde = desde, Hasta = hasta, Filas = filas };
    }

    public async Task<InformeResidentesCuidados> ObtenerResidentesCuidadosAsync(DateOnly desde, DateOnly hasta)
    {
        var dietasActivas = await _db.ResidenteDietas
            .Include(rd => rd.Dieta)
            .Include(rd => rd.Residente)
            .Where(rd => rd.FechaFin == null && rd.Residente!.Estado == EstadoResidente.Activo)
            .ToListAsync();

        var patologias = await _db.ResidentePatologias
            .Include(rp => rp.Patologia)
            .Include(rp => rp.Residente)
            .Where(rp => rp.Residente!.Estado == EstadoResidente.Activo)
            .ToListAsync();

        var desdeUtc = InicioUtc(desde);
        var hastaUtc = FinUtc(hasta);

        var administraciones = await _db.RegistrosAdministracion
            .Include(r => r.Medicacion).ThenInclude(m => m!.Residente)
            .Where(r => r.FechaHora >= desdeUtc && r.FechaHora <= hastaUtc)
            .ToListAsync();

        var sesiones = await _db.SesionesTerapia
            .Include(s => s.Residente)
            .Include(s => s.Empleado).ThenInclude(e => e!.Especialidad)
            .Where(s => s.Estado == EstadoSesion.Realizada && s.FechaHora >= desdeUtc && s.FechaHora <= hastaUtc)
            .ToListAsync();

        return new InformeResidentesCuidados
        {
            Desde = desde,
            Hasta = hasta,
            DietasActivas = dietasActivas.GroupBy(rd => rd.Dieta!.Nombre)
                .Select(g => (Dieta: g.Key, Cantidad: g.Count()))
                .OrderByDescending(x => x.Cantidad).ToList(),
            PatologiasFrecuentes = patologias.GroupBy(rp => rp.Patologia!.Nombre)
                .Select(g => (Patologia: g.Key, Cantidad: g.Count()))
                .OrderByDescending(x => x.Cantidad).ToList(),
            MedicacionAdministrada = administraciones
                .GroupBy(r => new { Residente = $"{r.Medicacion!.Residente!.Nombre} {r.Medicacion.Residente.Apellidos}", Medicamento = r.Medicacion.Nombre })
                .Select(g => new FilaMedicacionAdministrada { Residente = g.Key.Residente, Medicamento = g.Key.Medicamento, VecesAdministrada = g.Count() })
                .OrderByDescending(f => f.VecesAdministrada).ToList(),
            SesionesRealizadas = sesiones
                .GroupBy(s => new { Residente = $"{s.Residente!.Nombre} {s.Residente.Apellidos}", Profesional = s.Empleado!.Nombre, Especialidad = s.Empleado.Especialidad?.Nombre ?? "-" })
                .Select(g => new FilaSesionTerapia { Residente = g.Key.Residente, Profesional = g.Key.Profesional, Especialidad = g.Key.Especialidad, SesionesRealizadas = g.Count() })
                .OrderByDescending(f => f.SesionesRealizadas).ToList()
        };
    }

    public async Task<InformeActividadIncidencias> ObtenerActividadIncidenciasAsync(DateOnly desde, DateOnly hasta)
    {
        var desdeUtc = InicioUtc(desde);
        var hastaUtc = FinUtc(hasta);

        var avisos = await _db.EventosDistribucion
            .Where(e => e.RolDestino != null && e.FechaCreacion >= desdeUtc && e.FechaCreacion <= hastaUtc)
            .ToListAsync();

        var eventosResidente = await _db.AuditLogs
            .Where(a => a.EntidadTipo == "Residente" && (a.Accion == "Alta" || a.Accion == "Baja" || a.Accion == "Traslado")
                && a.FechaHora >= desdeUtc && a.FechaHora <= hastaUtc)
            .ToListAsync();

        return new InformeActividadIncidencias
        {
            Desde = desde,
            Hasta = hasta,
            AvisosPorDepartamento = avisos.GroupBy(e => e.RolDestino!.Value)
                .Select(g => new FilaAvisosPorDepartamento { Rol = g.Key, Total = g.Count(), Urgentes = g.Count(e => e.Urgente) })
                .OrderByDescending(f => f.Total).ToList(),
            TendenciaResidentes = eventosResidente.GroupBy(a => a.Accion)
                .Select(g => (Accion: g.Key, Cantidad: g.Count()))
                .OrderByDescending(x => x.Cantidad).ToList()
        };
    }
}
