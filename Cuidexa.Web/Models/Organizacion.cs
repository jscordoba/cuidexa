namespace Cuidexa.Web.Models;

// Entidad gestora que puede tener varios Centros físicos (ej. una fundación
// con sedes en distintas ciudades). Guarda la identidad de marca compartida
// por todos sus centros; los datos propios de cada edificio viven en Centro.
public class Organizacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? LogoRuta { get; set; }
    public string? ColorAcento { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Centro> Centros { get; set; } = new List<Centro>();
}
