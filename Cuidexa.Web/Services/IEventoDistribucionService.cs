using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IEventoDistribucionService
{
    // Solo por departamento — usado por el dashboard de tablet compartida
    // (Fase 6), que a propósito no debe ver avisos personales de un grupo.
    Task<List<EventoDistribucion>> ObtenerPorRolAsync(RolEmpleado rol);

    // Departamento + avisos personales dirigidos a este empleado (por un
    // grupo de notificación) — usado por los paneles de empleado normales.
    Task<List<EventoDistribucion>> ObtenerParaEmpleadoAsync(RolEmpleado rol, int empleadoId);

    Task MarcarLeidoAsync(int eventoId);

    // Aviso creado a mano por un empleado operativo (no por una acción del
    // sistema). Siempre llega a su propio departamento y a Admin, más los
    // departamentos adicionales que haya marcado — ver CrearAviso.cshtml.
    // residenteId es opcional: un aviso sobre el puesto de trabajo (equipo,
    // mantenimiento, insumos...) no tiene por qué estar ligado a nadie.
    // todoElCentro sustituye los destinos por rol por todos los que tienen
    // bandeja de avisos; grupoDestinoId añade una fila personal por cada
    // miembro activo de ese grupo, sin tocar la bandeja de su departamento.
    Task CrearAvisoManualAsync(int? residenteId, string descripcion, RolEmpleado rolOrigen,
        IEnumerable<RolEmpleado> rolesAdicionales, int? empleadoOrigenId,
        bool urgente, bool todoElCentro, int? grupoDestinoId);
}
