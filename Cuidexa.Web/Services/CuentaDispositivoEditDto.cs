using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public class CuentaDispositivoEditDto
{
    public string Nombre { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }

    // Solo se aplica si viene relleno: permite editar el resto de datos sin
    // obligar a escribir una contraseña nueva cada vez.
    public string? NuevaPassword { get; set; }
}
