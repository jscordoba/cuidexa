namespace Cuidexa.Web.Services;

// Vista de "qué medicación toca hoy": una fila por medicación activa,
// con si ya se registró una administración en el día de hoy.
public class AgendaMedicacionItem
{
    public int ResidenteId { get; set; }
    public string ResidenteNombre { get; set; } = string.Empty;
    public int MedicacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Dosis { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public bool AdministradaHoy { get; set; }
}
