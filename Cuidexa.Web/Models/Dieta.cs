namespace Cuidexa.Web.Models;

public class Dieta : ITieneOrganizacion
{
    public int Id { get; set; }

    // Catálogo compartido por todos los Centros de una misma Organizacion.
    public int OrganizacionId { get; set; }
    public Organizacion? Organizacion { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
