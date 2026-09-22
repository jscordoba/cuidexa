using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Solicitud de cambio del turno por defecto de un empleado en una fecha
// concreta. No referencia ningún Turno preexistente — con turnos por
// defecto ya no hace falta que exista una fila de Turno para poder pedir
// cambiarla, "mi turno" en esa fecha es simplemente mi PlantillaTurnoDefecto.
// Quien lo pide propone opcionalmente quién lo cubre; Admin confirma (o
// cambia) el sustituto al aprobar — así la propuesta del empleado es solo
// eso, una propuesta.
public class CambioTurno : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public DateOnly Fecha { get; set; }
    public RolEmpleado Rol { get; set; }

    public int EmpleadoSolicitanteId { get; set; }
    public Empleado? EmpleadoSolicitante { get; set; }
    public string Motivo { get; set; } = string.Empty;

    public int? EmpleadoSustitutoPropuestoId { get; set; }
    public Empleado? EmpleadoSustitutoPropuesto { get; set; }

    public EstadoCambioTurno Estado { get; set; } = EstadoCambioTurno.Pendiente;
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public DateTime? FechaResolucion { get; set; }
    public int? ResueltoPorId { get; set; }
    public Empleado? ResueltoPor { get; set; }
}
