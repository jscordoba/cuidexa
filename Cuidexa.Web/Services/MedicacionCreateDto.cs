namespace Cuidexa.Web.Services;

public class MedicacionCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Dosis { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public string? Instrucciones { get; set; }
}
