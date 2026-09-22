using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IProfesionalesService
{
    Task<List<Residente>> ObtenerResidentesAsync();
    Task<Residente?> ObtenerResidenteAsync(int residenteId);
    Task<List<SesionTerapia>> ObtenerSesionesAsync(int residenteId, int empleadoId);
    Task<List<SesionTerapia>> ObtenerAgendaAsync(int empleadoId);
    Task<List<AuditLog>> ObtenerHistorialAsync(int residenteId, int empleadoId);

    Task ProgramarSesionAsync(int residenteId, int empleadoId, DateTime fechaHora);
    Task MarcarRealizadaAsync(int sesionId, int empleadoId, string? observaciones);
    Task CancelarSesionAsync(int sesionId, int empleadoId);
}
