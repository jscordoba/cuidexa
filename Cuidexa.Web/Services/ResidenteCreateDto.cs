using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Datos que recoge el formulario de alta (Admin). Separado de la entidad
// para no atar el formulario web directamente a la estructura de la BD.
public class ResidenteCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? DocumentoIdentidad { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public TipoResidente TipoResidente { get; set; }

    public int? HabitacionId { get; set; }

    public Movilidad Movilidad { get; set; }
    public bool NecesitaAyudaLevantarse { get; set; }
    public string? EquipamientoEspecial { get; set; }

    public string? Telefono { get; set; }
    public string? Email { get; set; }

    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaRelacion { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public string? ContactoEmergenciaEmail { get; set; }

    public int DietaId { get; set; }
    public List<int> AlergiaIds { get; set; } = new();

    // Validación de negocio simple: solo el residente Fijo requiere habitación.
    // Vive aquí (no en atributos de validación) porque depende de otro campo del propio DTO.
    public bool EsValido(out string error)
    {
        if (TipoResidente == TipoResidente.Fijo && HabitacionId is null)
        {
            error = "Un residente Fijo debe tener una habitación asignada.";
            return false;
        }
        error = string.Empty;
        return true;
    }
}
