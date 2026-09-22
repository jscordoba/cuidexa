using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// El corazon del MVP: registra que informacion debe ver cada rol
// cuando ocurre un alta o cambio relevante en un residente.
public class EventoDistribucion : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    // Nullable: los avisos manuales pueden ser sobre el puesto de trabajo
    // (equipo, mantenimiento, insumos...) sin estar ligados a ningún
    // residente concreto. Los avisos generados por el sistema (alta/baja/
    // traslado/modificación) siempre lo llevan, porque nacen de una acción
    // sobre un residente.
    public int? ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public TipoEvento TipoEvento { get; set; }

    // Exactamente uno de RolDestino/EmpleadoDestinoId va relleno: la mayoría
    // de eventos (sistema y avisos a un departamento) usan RolDestino; los
    // avisos dirigidos a un GrupoNotificacion generan una fila por miembro
    // con EmpleadoDestinoId en su lugar, para no notificar a todo el
    // departamento de cada miembro.
    public RolEmpleado? RolDestino { get; set; }
    public int? EmpleadoDestinoId { get; set; }
    public Empleado? EmpleadoDestino { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Leido { get; set; }

    // Solo tiene efecto en avisos manuales (Fase 7) — los eventos de sistema
    // no tienen un concepto de urgencia propio, nadie lo pidió para ellos.
    public bool Urgente { get; set; }

    // Solo tienen valor para TipoEvento.AvisoManual — los eventos generados
    // automáticamente por el sistema (alta/baja/traslado/modificación) no
    // tienen un departamento "emisor", son de la propia aplicación.
    public RolEmpleado? RolOrigen { get; set; }
    public int? EmpleadoOrigenId { get; set; }
    public Empleado? EmpleadoOrigen { get; set; }
}
