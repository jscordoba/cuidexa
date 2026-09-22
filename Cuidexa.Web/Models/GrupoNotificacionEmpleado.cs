namespace Cuidexa.Web.Models;

// Tabla intermedia N:M entre GrupoNotificacion y Empleado.
public class GrupoNotificacionEmpleado : ITieneCentro
{
    // Denormalizado a partir de GrupoNotificacion.CentroId (ver ResidenteDieta).
    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int GrupoNotificacionId { get; set; }
    public GrupoNotificacion? GrupoNotificacion { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }
}
