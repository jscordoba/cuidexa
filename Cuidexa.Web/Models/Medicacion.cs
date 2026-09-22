namespace Cuidexa.Web.Models;

// Medicación de un residente. Vigente mientras FechaFin sea null (mismo
// patrón que ResidenteDieta), así queda historial de tratamientos pasados.
// Nombre/Dosis/Horario/Instrucciones van cifrados en reposo: es información
// clínica específica del residente, no un catálogo compartido.
public class Medicacion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Dosis { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public string? Instrucciones { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public ICollection<RegistroAdministracion> Registros { get; set; } = new List<RegistroAdministracion>();
}
