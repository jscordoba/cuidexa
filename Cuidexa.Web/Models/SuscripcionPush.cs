namespace Cuidexa.Web.Models;

// Suscripción de push (Web Push/VAPID) de un navegador/dispositivo de un
// empleado (Fase 7) — un empleado puede tener varias (uno por navegador).
// Deliberadamente solo para empleados, no para las tablets compartidas de
// la Fase 6 (esas ya se autoactualizan solas, no son "de alguien").
public class SuscripcionPush : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
