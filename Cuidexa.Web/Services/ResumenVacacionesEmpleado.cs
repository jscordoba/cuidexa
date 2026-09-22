using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Fila del resumen de vacaciones por empleado que ve Admin: cuántos días ya
// disfrutó (aprobadas y ya pasadas) y cuántos tiene pendientes de resolver.
public class ResumenVacacionesEmpleado
{
    public int EmpleadoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
    public int DiasDisfrutados { get; set; }
    public int DiasSolicitados { get; set; }
}
