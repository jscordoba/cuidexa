using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IAuditService
{
    // centroIdExplicito: solo hace falta cuando quien registra la acción no
    // tiene un Centro de sesión propio (SuperAdmin, ver SuperAdminController)
    // — en el resto de casos se omite y se usa el Centro de la sesión actual.
    Task RegistrarAsync(int? empleadoId, string accion, string entidadTipo, int entidadId, string detalle, int? centroIdExplicito = null);
    Task<List<AuditLog>> ObtenerPorEntidadAsync(string entidadTipo, int entidadId);
}
