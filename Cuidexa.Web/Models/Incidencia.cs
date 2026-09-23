using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Sistema propio, separado de EventoDistribucion (avisos) a propósito: una
// incidencia es un registro con estado que hay que resolver, no una
// notificación de leído/no leído. Funciona de forma parecida (avisa al área
// responsable según su gravedad, con push si es Urgente) pero es una fila
// única por incidencia — no se duplica por destinatario.
public class Incidencia : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public TipoIncidencia Tipo { get; set; }

    // Solo tiene sentido cuando Tipo == Residente.
    public int? ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public GravedadIncidencia Gravedad { get; set; }
    public EstadoIncidencia Estado { get; set; } = EstadoIncidencia.Abierta;

    // Informativa: se crea directamente Resuelta, sin acción pendiente ni
    // push — ver IncidenciaService.CrearAsync.
    public CaracterIncidencia Caracter { get; set; } = CaracterIncidencia.RequiereSeguimiento;

    // Quién la reporta — Admin siempre puede reportar/ver cualquiera además
    // del rol origen, igual que con los avisos manuales.
    public RolEmpleado RolOrigen { get; set; }
    public int? EmpleadoOrigenId { get; set; }
    public Empleado? EmpleadoOrigen { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaResolucion { get; set; }
    public string? NotasResolucion { get; set; }
    public int? ResueltoPorId { get; set; }
    public Empleado? ResueltoPor { get; set; }

    // Firmas de conformidad opcionales (ver DocumentoFirmado) — no toda
    // incidencia necesita una.
    public ICollection<DocumentoFirmado> DocumentosFirmados { get; set; } = new List<DocumentoFirmado>();
}
