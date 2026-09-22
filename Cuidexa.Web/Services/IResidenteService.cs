using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IResidenteService
{
    Task<List<Residente>> ObtenerTodosAsync();
    Task<Residente?> ObtenerPorIdAsync(int id);

    // Igual que ObtenerPorIdAsync pero con TODA la información asociada al
    // residente entre departamentos (medicación/patologías de Enfermería,
    // sesiones de Profesionales...) — solo para la ficha completa (Admin),
    // no para el uso normal de ObtenerPorIdAsync (edición, listados), que no
    // necesita cargar tanto.
    Task<Residente?> ObtenerFichaCompletaAsync(int id);
    // centroId: solo lo pasa DirectorOrganizacion (ver IEmpleadoService.CrearAsync).
    Task<Residente> DarDeAltaAsync(ResidenteCreateDto dto, int? empleadoId, int? centroId = null);
    Task DarDeBajaAsync(int residenteId, int? empleadoId, string? motivo);
    Task TrasladarAsync(int residenteId, int nuevaHabitacionId, int? empleadoId);
    Task ActualizarAsync(int residenteId, ResidenteEditDto dto, int? empleadoId);
}
