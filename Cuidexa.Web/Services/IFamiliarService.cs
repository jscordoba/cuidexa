using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IFamiliarService
{
    Task<List<Familiar>> ObtenerPorResidenteAsync(int residenteId);
    Task CrearAsync(int residenteId, string nombre, string relacion, string email, string password, int? actorEmpleadoId);

    // Datos que ve el propio familiar — solo lo acordado: documentos
    // firmados e incidencias ya resueltas de SU residente, nada más.
    Task<List<DocumentoFirmado>> ObtenerDocumentosAsync(int residenteId);
    Task<List<Incidencia>> ObtenerIncidenciasResueltasAsync(int residenteId);
}
