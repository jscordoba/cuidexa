using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IEmpleadoService
{
    Task<List<Empleado>> ObtenerTodosAsync();
    Task<Empleado?> ObtenerPorIdAsync(int id);
    // centroId: solo lo pasa DirectorOrganizacion al elegir a qué centro de
    // su organización pertenece el nuevo empleado (Fase 9) — Admin nunca lo
    // pasa, y se usa el propio centro de su sesión.
    Task<Empleado> CrearAsync(EmpleadoCreateDto dto, int? empleadoIdActor, int? centroId = null);
    Task ActualizarAsync(int id, EmpleadoEditDto dto, int? empleadoIdActor);
    Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor);

    // Usado solo por SuperAdminController al provisionar un Centro nuevo: a
    // diferencia de CrearAsync, no depende de ITenantContext (quien lo llama
    // no tiene sesión de Centro propia) — el centro destino se pasa explícito.
    Task<Empleado> CrearAdminInicialAsync(int centroId, string nombre, string email, string password);
}
