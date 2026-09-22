namespace Cuidexa.Web.Services;

public interface IInformesService
{
    Task<InformeTurnosPersonal> ObtenerTurnosPersonalAsync(DateOnly desde, DateOnly hasta);
    Task<InformeResidentesCuidados> ObtenerResidentesCuidadosAsync(DateOnly desde, DateOnly hasta);
    Task<InformeActividadIncidencias> ObtenerActividadIncidenciasAsync(DateOnly desde, DateOnly hasta);
}
