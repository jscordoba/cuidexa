using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IPushNotificationService
{
    // Resuelve destinatarios de cada evento (por EmpleadoDestinoId o por
    // RolDestino) y envía un push real a cada suscripción activa que tengan.
    Task NotificarAsync(IEnumerable<EventoDistribucion> eventosNuevos);

    Task SuscribirAsync(int empleadoId, string endpoint, string p256dh, string auth);
    Task DesuscribirAsync(string endpoint);
}
