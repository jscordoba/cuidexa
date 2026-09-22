using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Entidad central del sistema: el usuario/residente del centro asistencial.
public class Residente : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    // --- Datos básicos ---
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? DocumentoIdentidad { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public TipoResidente TipoResidente { get; set; }

    // Habitación: obligatoria solo para residentes Fijos (se valida en el servicio,
    // no en la BD, porque la regla puede evolucionar sin tocar el esquema).
    public int? HabitacionId { get; set; }
    public Habitacion? Habitacion { get; set; }

    public Movilidad Movilidad { get; set; }
    public bool NecesitaAyudaLevantarse { get; set; }
    public string? EquipamientoEspecial { get; set; }

    public EstadoResidente Estado { get; set; } = EstadoResidente.Activo;
    public DateOnly FechaIngreso { get; set; }

    // --- Datos de contacto del propio residente ---
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // --- Contacto de emergencia ---
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaRelacion { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public string? ContactoEmergenciaEmail { get; set; }

    public ICollection<ResidenteDieta> Dietas { get; set; } = new List<ResidenteDieta>();
    public ICollection<ResidenteAlergia> Alergias { get; set; } = new List<ResidenteAlergia>();
    public ICollection<ResidentePatologia> Patologias { get; set; } = new List<ResidentePatologia>();
    public ICollection<Medicacion> Medicaciones { get; set; } = new List<Medicacion>();
    public ICollection<SesionTerapia> SesionesTerapia { get; set; } = new List<SesionTerapia>();
    public ICollection<EventoDistribucion> Eventos { get; set; } = new List<EventoDistribucion>();
}
