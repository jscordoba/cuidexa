using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IRolPresentacionService
{
    (string Icono, string ClaseColor, string Etiqueta) Obtener(RolEmpleado rol);
}
