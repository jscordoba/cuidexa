namespace Cuidexa.Web.Models;

// Implementada por los catálogos compartidos entre todos los Centros de una
// misma Organizacion (Dieta, Alergia, Patologia, Especialidad, PlantillaTurno
// — Fase 9). Ver ITieneCentro para el mismo patrón a nivel de Centro.
public interface ITieneOrganizacion
{
    int OrganizacionId { get; set; }
    Organizacion? Organizacion { get; set; }
}
