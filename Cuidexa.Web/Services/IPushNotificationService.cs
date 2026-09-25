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

    // Fase 10 — comercialización: SuperAdmin (bloque 3, tickets de soporte
    // nuevos) y Familiar (bloque 5, documentos/incidencias nuevas de su
    // residente) tienen sus propias suscripciones — no son Empleado.
    Task SuscribirSuperAdminAsync(int superAdminId, string endpoint, string p256dh, string auth);
    Task DesuscribirSuperAdminAsync(string endpoint);
    Task SuscribirFamiliarAsync(int familiarId, string endpoint, string p256dh, string auth);
    Task DesuscribirFamiliarAsync(string endpoint);

    Task NotificarNuevoTicketSoporteAsync(TicketSoporte ticket);

    // titulo/cuerpo ya formados por quien llama (DocumentoFirmadoService/
    // IncidenciaService) — a diferencia de NotificarAsync, aquí no hay un
    // "tipo de evento" común que decida el mensaje por sí solo.
    Task NotificarFamiliarAsync(int residenteId, string titulo, string cuerpo);
}
