using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface ISoporteService
{
    Task<List<TicketSoporte>> ObtenerParaCentroActualAsync();
    Task CrearTicketAsync(string titulo, string descripcion, int? empleadoId);

    // Sin filtro de tenant — solo para el panel de SuperAdmin.
    Task<List<TicketSoporte>> ObtenerTodosAsync();
    Task ResponderAsync(int ticketId, string respuesta, int superAdminId);
}
