using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IFestivosApiService
{
    // Festivos nacionales + propios de la comunidad para ese año (sin
    // festivos locales de municipio, que este MVP no modela). Lanza
    // InvalidOperationException con un mensaje apto para mostrar al usuario
    // si el servicio externo falla o responde en un formato inesperado.
    Task<List<(DateOnly Fecha, string Nombre)>> ObtenerFestivosAsync(int anio, ComunidadAutonoma comunidad);
}
