namespace Cuidexa.Web.Services;

// Fase 10 — "IA para informes/predicción de personal", acotado a análisis
// local (estadística/heurística sobre los datos ya guardados) sin ninguna
// llamada a servicios de IA externos: no se envían datos del centro a
// terceros.
public interface IAnalisisService
{
    Task<List<string>> ObtenerResumenEjecutivoAsync(
        InformeTurnosPersonal turnos, InformeResidentesCuidados residentes, InformeActividadIncidencias actividad);

    Task<List<string>> DetectarPatronesIncidenciasAsync(DateOnly desde, DateOnly hasta);

    Task<AnalisisPersonal> PredecirPersonalNecesarioAsync();
}
