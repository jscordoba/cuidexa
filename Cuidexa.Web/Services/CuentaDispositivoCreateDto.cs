using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public class CuentaDispositivoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
}
