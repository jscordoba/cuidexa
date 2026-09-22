using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Turno de trabajo por departamento (Rol). EmpleadoId es el titular; si el
// turno lo cubre alguien ajeno a la plantilla (asistencia externa) se deja
// EmpleadoId a null y se usa NombreExterno en su lugar — evita crear un
// catálogo de "personal externo" completo para este MVP.
public class Turno : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public RolEmpleado Rol { get; set; }

    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }
    public string? NombreExterno { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.Programado;
    public string? Notas { get; set; }

    // Opcional: de qué plantilla (Mañana/Tarde/Noche...) se generaron las
    // horas de este turno — solo para autorellenar el formulario, el turno
    // sigue teniendo sus propias FechaInicio/FechaFin libres.
    public int? PlantillaTurnoId { get; set; }
    public PlantillaTurno? PlantillaTurno { get; set; }

    // Reemplazo asignado directamente por Admin (no por solicitud del propio
    // empleado) para cubrir la ausencia de otra persona en una franja
    // concreta — se resalta en "Mis turnos" a modo de notificación.
    public bool EsReemplazo { get; set; }
    public string? MotivoReemplazo { get; set; }

    // Cambio de turno entre compañeros ya aprobado (ver CambioTurno) — igual
    // que EsReemplazo pero originado por el propio empleado, no por Admin.
    // Cuando es true, EmpleadoOriginalId indica a quién se le "quitó" el
    // turno ese día (su plantilla por defecto deja de aplicar esa fecha).
    public bool EsCambio { get; set; }
    public int? EmpleadoOriginalId { get; set; }
    public Empleado? EmpleadoOriginal { get; set; }
}
