namespace Cuidexa.Web.Models;

// Registro de cada vez que se administra una dosis: el "historial de
// administración" que Enfermería necesita para saber qué se dio y cuándo.
public class RegistroAdministracion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int MedicacionId { get; set; }
    public Medicacion? Medicacion { get; set; }

    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}
