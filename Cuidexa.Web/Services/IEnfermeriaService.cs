using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public interface IEnfermeriaService
{
    Task<List<Residente>> ObtenerResidentesAsync();
    Task<Residente?> ObtenerDetalleAsync(int residenteId);
    Task<List<AuditLog>> ObtenerHistorialClinicoAsync(int residenteId);
    Task<List<AgendaMedicacionItem>> ObtenerAgendaHoyAsync();

    Task AgregarPatologiaAsync(int residenteId, int patologiaId, string? observaciones, int? empleadoId);
    Task QuitarPatologiaAsync(int residenteId, int patologiaId, int? empleadoId);

    Task AgregarMedicacionAsync(int residenteId, MedicacionCreateDto dto, int? empleadoId);
    Task FinalizarMedicacionAsync(int medicacionId, int? empleadoId);
    Task RegistrarAdministracionAsync(int medicacionId, string? observaciones, int? empleadoId);
}
