using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class AnalisisService : IAnalisisService
{
    private readonly CuidexaDbContext _db;
    private readonly ITurnoService _turnos;

    public AnalisisService(CuidexaDbContext db, ITurnoService turnos)
    {
        _db = db;
        _turnos = turnos;
    }

    public Task<List<string>> ObtenerResumenEjecutivoAsync(
        InformeTurnosPersonal turnos, InformeResidentesCuidados residentes, InformeActividadIncidencias actividad)
    {
        var puntos = new List<string>();

        if (turnos.Filas.Any())
        {
            var top = turnos.Filas.OrderByDescending(f => f.HorasTrabajadas).First();
            puntos.Add($"{top.Nombre} ({top.Rol}) acumula más horas trabajadas del periodo: {top.HorasTrabajadas:0.0}h.");

            var ausencias = turnos.Filas.Sum(f => f.DiasAusencia);
            if (ausencias > 0)
            {
                puntos.Add($"{ausencias} día(s)-persona de ausencia cubiertos por compañeros en el periodo.");
            }
        }

        if (residentes.DietasActivas.Any())
        {
            var dietaTop = residentes.DietasActivas.First();
            puntos.Add($"La dieta más común entre los residentes activos es \"{dietaTop.Dieta}\" ({dietaTop.Cantidad} residentes).");
        }

        if (residentes.PatologiasFrecuentes.Any())
        {
            var patTop = residentes.PatologiasFrecuentes.First();
            puntos.Add($"La patología más frecuente es \"{patTop.Patologia}\" ({patTop.Cantidad} residentes).");
        }

        if (actividad.AvisosPorDepartamento.Any())
        {
            var deptTop = actividad.AvisosPorDepartamento.OrderByDescending(f => f.Urgentes).First();
            if (deptTop.Urgentes > 0)
            {
                puntos.Add($"{deptTop.Rol} fue el departamento con más avisos urgentes del periodo ({deptTop.Urgentes}).");
            }
        }

        if (!puntos.Any())
        {
            puntos.Add("Sin actividad suficiente en el rango seleccionado para destacar nada.");
        }

        return Task.FromResult(puntos);
    }

    public async Task<List<string>> DetectarPatronesIncidenciasAsync(DateOnly desde, DateOnly hasta)
    {
        var desdeUtc = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hastaUtc = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var incidencias = await _db.Incidencias
            .Include(i => i.Residente)
            .Where(i => i.FechaCreacion >= desdeUtc && i.FechaCreacion <= hastaUtc)
            .ToListAsync();

        var patrones = new List<string>();
        if (!incidencias.Any())
        {
            return patrones;
        }

        // Concentración por residente: 3+ incidencias del mismo residente en el rango.
        var porResidente = incidencias
            .Where(i => i.ResidenteId.HasValue)
            .GroupBy(i => i.ResidenteId!.Value)
            .Where(g => g.Count() >= 3)
            .OrderByDescending(g => g.Count());
        foreach (var g in porResidente)
        {
            var residente = g.First().Residente;
            var nombre = residente is not null ? $"{residente.Nombre} {residente.Apellidos}" : "Un residente";
            var tipoMasComun = g.GroupBy(i => i.Tipo).OrderByDescending(x => x.Count()).First().Key;
            patrones.Add($"{nombre} concentra {g.Count()} incidencias en el rango (mayormente de tipo {EtiquetaTipo(tipoMasComun)}) — puede convenir revisar su plan de cuidados.");
        }

        // Día de la semana que concentra más incidencias graves/urgentes que el resto.
        var graves = incidencias.Where(i => i.Gravedad is GravedadIncidencia.Urgente or GravedadIncidencia.Alta).ToList();
        if (graves.Count >= 4)
        {
            var porDia = graves.GroupBy(i => i.FechaCreacion.DayOfWeek)
                .Select(g => (Dia: g.Key, Cantidad: g.Count()))
                .OrderByDescending(x => x.Cantidad)
                .First();
            var mediaPorDia = graves.Count / 7.0;
            if (porDia.Cantidad >= 2 && porDia.Cantidad >= mediaPorDia * 1.75)
            {
                patrones.Add($"Los {EtiquetaDia(porDia.Dia)} concentran más incidencias graves/urgentes que el resto de la semana ({porDia.Cantidad} de {graves.Count}) — puede convenir reforzar personal ese día.");
            }
        }

        // Tipo de incidencia dominante del periodo.
        var porTipo = incidencias.GroupBy(i => i.Tipo).OrderByDescending(g => g.Count()).First();
        if (incidencias.Count >= 4 && porTipo.Count() >= incidencias.Count * 0.5)
        {
            patrones.Add($"Más de la mitad de las incidencias del periodo son de tipo {EtiquetaTipo(porTipo.Key)} ({porTipo.Count()} de {incidencias.Count}).");
        }

        return patrones;
    }

    public async Task<AnalisisPersonal> PredecirPersonalNecesarioAsync()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Ventana histórica: hasta 8 semanas hacia atrás (limitado por los
        // datos que realmente existan — sin error si hay menos).
        var historico = await _turnos.ObtenerTurnosEfectivosAsync(hoy.AddDays(-56), hoy.AddDays(-1), null);
        var historicoTrabajado = historico.Where(t => t.EmpleadoId.HasValue && !t.Cedido).ToList();

        // Promedio de personal distinto trabajando por (día de la semana, rol).
        var promedios = historicoTrabajado
            .GroupBy(t => (t.Fecha.DayOfWeek, t.Rol))
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(t => t.Fecha).Select(dia => dia.Select(x => x.EmpleadoId).Distinct().Count()).Average());

        var desde = hoy;
        var hasta = hoy.AddDays(6);
        var programado = await _turnos.ObtenerTurnosEfectivosAsync(desde, hasta, null);
        var programadoTrabajado = programado.Where(t => t.EmpleadoId.HasValue && !t.Cedido).ToList();

        var roles = historicoTrabajado.Select(t => t.Rol)
            .Concat(programadoTrabajado.Select(t => t.Rol))
            .Distinct();

        var filas = new List<FilaPrediccionPersonal>();
        for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
        {
            foreach (var rol in roles)
            {
                var recomendado = promedios.TryGetValue((fecha.DayOfWeek, rol), out var avg) ? avg : 0;
                var programadoCount = programadoTrabajado
                    .Where(t => t.Fecha == fecha && t.Rol == rol)
                    .Select(t => t.EmpleadoId)
                    .Distinct()
                    .Count();

                if (recomendado <= 0 && programadoCount == 0)
                {
                    continue;
                }

                filas.Add(new FilaPrediccionPersonal
                {
                    Fecha = fecha,
                    Rol = rol,
                    PersonalProgramado = programadoCount,
                    PersonalRecomendado = Math.Round(recomendado, 1),
                    // Margen de 0.5 para no marcar como "falta" una diferencia de solo redondeo.
                    PosibleFaltaPersonal = programadoCount < recomendado - 0.5
                });
            }
        }

        return new AnalisisPersonal
        {
            Desde = desde,
            Hasta = hasta,
            Filas = filas.OrderBy(f => f.Fecha).ThenBy(f => f.Rol).ToList()
        };
    }

    private static string EtiquetaTipo(TipoIncidencia t) => t switch
    {
        TipoIncidencia.Residente => "Residente",
        TipoIncidencia.Personal => "Personal/operativa",
        TipoIncidencia.Instalaciones => "Instalaciones/seguridad",
        _ => t.ToString()
    };

    private static string EtiquetaDia(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday => "lunes",
        DayOfWeek.Tuesday => "martes",
        DayOfWeek.Wednesday => "miércoles",
        DayOfWeek.Thursday => "jueves",
        DayOfWeek.Friday => "viernes",
        DayOfWeek.Saturday => "sábados",
        DayOfWeek.Sunday => "domingos",
        _ => d.ToString()
    };
}
