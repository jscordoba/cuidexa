using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Vacaciones de un empleado. El Admin puede registrarlas directamente (queda
// Aprobada de inmediato) o el propio empleado puede solicitarlas desde "Mis
// turnos" (queda Solicitada hasta que Admin resuelve) — mismo patrón que
// CambioTurno para las solicitudes de cambio de turno.
public class Vacacion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public EstadoVacacion Estado { get; set; } = EstadoVacacion.Solicitada;
    public string? Notas { get; set; }

    // Por defecto los días de vacaciones se cuentan solo en días laborables
    // (sin fines de semana ni festivos) — Admin puede marcar esta bandera,
    // al registrar o al aprobar, para contar el rango completo en su lugar.
    public bool IncluyeFinesSemanaYFestivos { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }
    public int? ResueltoPorId { get; set; }
    public Empleado? ResueltoPor { get; set; }
}
