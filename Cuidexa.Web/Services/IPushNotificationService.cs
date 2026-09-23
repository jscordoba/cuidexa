using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IPushNotificationService
{
    // Resuelve destinatarios de cada evento (por EmpleadoDestinoId o por
    // RolDestino) y envía un push real a cada suscripción activa que tengan.
    Task NotificarAsync(IEnumerable<EventoDistribucion> eventosNuevos);

    // Solo para incidencias Urgente (ver IncidenciaService.CrearAsync) — una
    // única fila, así que a diferencia de NotificarAsync no hace falta
    // iterar una lista, solo el rol responsable de resolverla.
    Task NotificarIncidenciaAsync(Incidencia incidencia, RolEmpleado rolDestino);

    Task SuscribirAsync(int empleadoId, string endpoint, string p256dh, string auth);
    Task DesuscribirAsync(string endpoint);
}
