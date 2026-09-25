namespace Cuidexa.Web.Models;

// Suscripción de push de un SuperAdmin (Fase 10 — comercialización, bloque
// 3: notificar tickets de soporte nuevos). Tabla propia, no reutiliza
// SuscripcionPush: SuperAdmin no tiene CentroId (esa tabla es ITieneCentro),
// mismo motivo por el que ya tiene su propio esquema de cookie y sus
// propias tablas en el resto de la app.
public class SuscripcionPushSuperAdmin
{
    public int Id { get; set; }

    public int SuperAdminId { get; set; }
    public SuperAdmin? SuperAdmin { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
