using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface ILimpiezaService
{
    Task<List<TareaLimpieza>> ObtenerPendientesAsync();
    Task<List<TareaLimpieza>> ObtenerCompletadasRecientesAsync();
    Task CrearTareaZonaAsync(string zona, string descripcion);
    Task MarcarCompletadaAsync(int tareaId, int? empleadoId);
}
