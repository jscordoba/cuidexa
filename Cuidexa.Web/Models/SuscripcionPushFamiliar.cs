namespace Cuidexa.Web.Models;

// Suscripción de push de un Familiar (Fase 10 — comercialización, bloque 5:
// notificar documentos/incidencias nuevas de su residente). Tabla propia,
// no reutiliza SuscripcionPush: Familiar no tiene CentroId ni es un
// Empleado — mismo motivo por el que ya tiene su propio esquema de cookie.
public class SuscripcionPushFamiliar
{
    public int Id { get; set; }

    public int FamiliarId { get; set; }
    public Familiar? Familiar { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
