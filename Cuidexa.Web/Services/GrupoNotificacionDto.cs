namespace Cuidexa.Web.Services;

public class GrupoNotificacionDto
{
    public string Nombre { get; set; } = string.Empty;
    public List<int> EmpleadoIds { get; set; } = new();
}
