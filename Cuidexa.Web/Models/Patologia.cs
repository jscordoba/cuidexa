namespace Cuidexa.Web.Models;

// Catálogo de patologías (igual que Dieta/Alergia). El nombre va en claro
// porque es un catálogo compartido, no un dato específico del residente.
public class Patologia : ITieneOrganizacion
{
    public int Id { get; set; }

    // Catálogo compartido por todos los Centros de una misma Organizacion.
    public int OrganizacionId { get; set; }
    public Organizacion? Organizacion { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
