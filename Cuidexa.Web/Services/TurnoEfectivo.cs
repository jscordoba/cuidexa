using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// El turno que un empleado realmente cubre en una fecha concreta: o bien su
// PlantillaTurnoDefecto (caso normal, sin fila de Turno en BD), o bien una
// excepción real (reemplazo/cambio), o "cedido" si ese día le cubrieron el
// turno a otro. Una sola fuente de verdad para Calendario y Comensales.
public class TurnoEfectivo
{
    // Null cuando el turno lo cubre personal externo (sin ficha de empleado).
    public int? EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
    public DateOnly Fecha { get; set; }

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public bool EsExcepcion { get; set; }

    // "Reemplazo" | "Cambio" | null (turno por defecto, sin excepción)
    public string? TipoExcepcion { get; set; }

    // Solo si EsExcepcion: motivo registrado en el Turno de excepción.
    public string? Notas { get; set; }

    // Solo si ese día le cedieron su turno por defecto a otro compañero —
    // no cuenta como "trabajando" ese día.
    public bool Cedido { get; set; }
    public string? CedidoA { get; set; }
}
