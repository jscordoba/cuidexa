using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface ICuentaDispositivoService
{
    Task<List<CuentaDispositivo>> ObtenerTodasAsync();
    Task<CuentaDispositivo?> ObtenerPorIdAsync(int id);
    // centroId: solo lo pasa DirectorOrganizacion (ver IEmpleadoService.CrearAsync).
    Task<CuentaDispositivo> CrearAsync(CuentaDispositivoCreateDto dto, int? empleadoIdActor, int? centroId = null);
    Task ActualizarAsync(int id, CuentaDispositivoEditDto dto, int? empleadoIdActor);
    Task CambiarEstadoAsync(int id, bool activo, int? empleadoIdActor);

    // Login de la tablet: null si no existe, está inactiva, o la contraseña
    // no coincide — un único resultado para no filtrar cuál de los tres pasó.
    // Se acota explícitamente por centroId (Fase 9): en este punto de la
    // petición no hay sesión todavía, así que el filtro global no aplica.
    Task<CuentaDispositivo?> AutenticarAsync(string nombre, string password, int centroId);

    Task<DispositivoDashboardViewModel> ObtenerResumenAsync(CuentaDispositivo cuenta);
}
