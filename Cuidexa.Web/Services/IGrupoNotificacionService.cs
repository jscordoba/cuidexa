using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IGrupoNotificacionService
{
    Task<List<GrupoNotificacion>> ObtenerTodosAsync();
    Task<List<GrupoNotificacion>> ObtenerActivosAsync();
    Task<GrupoNotificacion?> ObtenerPorIdAsync(int id);
    Task<List<Empleado>> ObtenerMiembrosAsync(int grupoId);
    // centroId: solo lo pasa DirectorOrganizacion (ver IEmpleadoService.CrearAsync)
    // — el grupo entero pertenece a un único centro, igual que los avisos que genera.
    Task<GrupoNotificacion> CrearAsync(GrupoNotificacionDto dto, int? empleadoIdActor, int? centroId = null);
    Task ActualizarAsync(int id, GrupoNotificacionDto dto, int? empleadoIdActor);
    Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor);
}
