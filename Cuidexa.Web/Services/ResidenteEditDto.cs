using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Datos editables de un residente ya existente. Deliberadamente sin
// HabitacionId ni Estado: esos cambios de estado tienen su propio flujo
// (Trasladar / Baja) porque generan avisos y auditoría específicos —
// mezclarlos aquí perdería esa trazabilidad.
public class ResidenteEditDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? DocumentoIdentidad { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public TipoResidente TipoResidente { get; set; }

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
}
