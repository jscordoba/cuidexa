using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IVacacionService
{
    Task<List<Vacacion>> ObtenerTodasAsync();
    Task<List<Vacacion>> ObtenerPorEmpleadoAsync(int empleadoId);
    Task<List<Vacacion>> ObtenerPendientesAsync();
    Task<List<ResumenVacacionesEmpleado>> ObtenerResumenAsync();

    // Alta directa de Admin: queda aprobada de inmediato.
    Task RegistrarAsync(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string? notas, bool incluyeFinesSemanaYFestivos, int? actorId);

    // Autoservicio del empleado: queda pendiente de resolución de Admin. El
    // empleado no decide si cuentan fines de semana/festivos — eso es
    // siempre una decisión de Admin, tomada al aprobar.
    Task SolicitarAsync(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string? notas);

    Task AprobarAsync(int vacacionId, bool incluyeFinesSemanaYFestivos, int? actorId);
    Task RechazarAsync(int vacacionId, int? actorId);

    // Festivos del centro, usados para excluirlos del cómputo de días.
    Task<List<Feriado>> ObtenerFeriadosAsync();
    Task<HashSet<DateOnly>> ObtenerFechasFeriadosAsync();
    Task CrearFeriadoAsync(DateOnly fecha, string? nombre);
    Task EliminarFeriadoAsync(int id);

    // Alta masiva desde una fuente externa (ver IFestivosApiService) — omite
    // en silencio las fechas que ya existan en vez de fallar en la primera.
    Task<(int Importados, int Omitidos)> ImportarFeriadosAsync(List<(DateOnly Fecha, string Nombre)> festivos, int? actorId);
}
