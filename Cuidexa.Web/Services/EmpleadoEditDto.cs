using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public class EmpleadoEditDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }

    // Solo tiene efecto cuando Rol == Profesional.
    public int? EspecialidadId { get; set; }

    // Obligatorio para los 5 roles operativos (validado en EmpleadoService);
    // null para Admin/Dirección.
    public int? PlantillaTurnoDefectoId { get; set; }

    // Solo se aplica si viene relleno: permite editar el resto de datos
    // sin obligar a escribir una contraseña nueva cada vez.
    public string? NuevaPassword { get; set; }
}
