using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public class EmpleadoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }

    // Solo tiene efecto cuando Rol == Profesional.
    public int? EspecialidadId { get; set; }

    // Obligatorio para los 5 roles operativos (validado en EmpleadoService);
    // null para Admin/Dirección.
    public int? PlantillaTurnoDefectoId { get; set; }
}
