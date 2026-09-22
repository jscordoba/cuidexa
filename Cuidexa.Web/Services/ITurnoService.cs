using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface ITurnoService
{
    // Administración (Admin) — CRUD de excepciones puntuales (reemplazos,
    // ajustes manuales). Los días "normales" no tienen fila propia, ver
    // ObtenerTurnosEfectivosAsync.
    Task<List<Turno>> ObtenerTodosAsync();
    // centroId: solo se usa cuando no hay empleadoId (turno "externo") y
    // quien crea es DirectorOrganizacion eligiendo centro (ver ICentroService).
    // Si hay empleadoId, el centro se deriva siempre de ese empleado.
    Task<Turno> CrearTurnoAsync(RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas, int? actorId, int? centroId = null);
    Task<Turno?> ObtenerPorIdAsync(int turnoId);
    Task EditarTurnoAsync(int turnoId, RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas, int? actorId);
    Task CancelarTurnoAsync(int turnoId, int? actorId);
    Task<List<CambioTurno>> ObtenerCambiosPendientesAsync();
    Task AprobarCambioAsync(int cambioId, int empleadoSustitutoId, int? actorId);
    Task RechazarCambioAsync(int cambioId, int? actorId);
    Task<Turno> CrearReemplazoAsync(RolEmpleado rol, int empleadoDestinoId, DateTime fechaInicio, DateTime fechaFin, string motivo, string? ausente, int? actorId);
    Task<List<Turno>> ObtenerReemplazosAsync();

    // Turno realmente cubierto por cada empleado en un rango de fechas: su
    // plantilla por defecto salvo que haya una excepción (reemplazo/cambio)
    // ese día — fuente única para el Calendario y para Comensales.
    Task<List<TurnoEfectivo>> ObtenerTurnosEfectivosAsync(DateOnly desde, DateOnly hasta, RolEmpleado? filtroRol);

    // Vista propia del empleado (todos los roles operativos)
    Task<List<Empleado>> ObtenerCompanerosAsync(RolEmpleado rolPropio, int excluirEmpleadoId);
    Task<List<Turno>> ObtenerTurnosEspecialesAsync(int empleadoId);
    Task<List<CambioTurno>> ObtenerCambiosDeAsync(int empleadoId);
    Task SolicitarCambioAsync(DateOnly fecha, int empleadoSolicitanteId, string motivo, int? empleadoSustitutoPropuestoId);
}
