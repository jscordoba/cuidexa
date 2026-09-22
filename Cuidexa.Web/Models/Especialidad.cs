namespace Cuidexa.Web.Models;

// Catálogo de especialidades (fisioterapia, logopedia...). Un único rol
// "Profesional" cubre todas ellas — la especialidad concreta distingue
// quién es quién, evitando un rol/controlador distinto por disciplina.
public class Especialidad : ITieneOrganizacion
{
    public int Id { get; set; }

    // Catálogo compartido por todos los Centros de una misma Organizacion.
    public int OrganizacionId { get; set; }
    public Organizacion? Organizacion { get; set; }

    public string Nombre { get; set; } = string.Empty;
}
