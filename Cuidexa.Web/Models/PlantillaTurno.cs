using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Franja horaria reutilizable (mañana/tarde/noche...) que Admin configura una
// vez y luego usa para crear/editar turnos con horas consistentes. Se
// desactiva en vez de borrarse para no romper turnos históricos que la
// referencian (ver PlantillaTurnoId en Turno).
public class PlantillaTurno : ITieneOrganizacion
{
    public int Id { get; set; }

    // Catálogo compartido por todos los Centros de una misma Organizacion.
    public int OrganizacionId { get; set; }
    public Organizacion? Organizacion { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public TipoDia TipoDia { get; set; } = TipoDia.Laborable;
    public bool Activo { get; set; } = true;
}
