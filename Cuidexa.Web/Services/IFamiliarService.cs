using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IFamiliarService
{
    Task<List<Familiar>> ObtenerPorResidenteAsync(int residenteId);
    Task CrearAsync(int residenteId, string nombre, string relacion, string email, string password, int? actorEmpleadoId);

    // Revocación de acceso: desactivar bloquea futuros logins de inmediato
    // (igual que Empleado.Activo/CuentaDispositivo.Activo — una sesión ya
    // iniciada no se invalida al momento, mismo límite ya aceptado en el
    // resto de la app con cookies sin estado del lado servidor).
    //
    // residenteId es obligatorio y se valida contra familiar.ResidenteId:
    // Familiar no lleva filtro de tenant (a propósito, ver arriba), así que
    // sin esta comprobación un Admin podría desactivar/restablecer la
    // contraseña de un familiar de un residente de OTRO centro con solo
    // adivinar su Id — el residenteId ya viene validado por el controlador
    // contra el tenant actual antes de llegar aquí.
    Task CambiarEstadoAsync(int residenteId, int familiarId, bool activo, int? actorEmpleadoId);
    Task RestablecerPasswordAsync(int residenteId, int familiarId, string nuevaPassword, int? actorEmpleadoId);

    // Datos que ve el propio familiar — solo lo acordado: documentos
    // firmados e incidencias ya resueltas de SU residente, nada más.
    Task<List<DocumentoFirmado>> ObtenerDocumentosAsync(int residenteId);
    Task<List<Incidencia>> ObtenerIncidenciasResueltasAsync(int residenteId);
}
