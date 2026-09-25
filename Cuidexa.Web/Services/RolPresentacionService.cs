using Cuidexa.Web.Models.Enums;
using Microsoft.Extensions.Localization;

namespace Cuidexa.Web.Services;

// Icono, color y etiqueta de cada rol para la interfaz (avatar de saludo,
// barra lateral de escritorio...) — en un único sitio para que las distintas
// vistas que necesitan "pintar" un rol nunca se desincronicen entre sí.
//
// Antes era ViewHelpers/RolPresentacion.cs (clase estática) — convertida a
// servicio inyectable para poder traducir Etiqueta vía IStringLocalizer
// (multi-idioma, bloque 6 de COMERCIALIZACION.md). Registrada como
// "RolPresentacion" en _ViewImports.cshtml, así que las ~25 vistas que ya
// hacían `RolPresentacion.Obtener(...)` no necesitan tocarse: la instancia
// inyectada ocupa el mismo nombre que antes ocupaba la clase estática.
public class RolPresentacionService : IRolPresentacionService
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RolPresentacionService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public (string Icono, string ClaseColor, string Etiqueta) Obtener(RolEmpleado rol) => rol switch
    {
        RolEmpleado.Admin => ("bi-shield-lock-fill", "avatar-admin", _localizer["Administración"]),
        RolEmpleado.Cocina => ("bi-egg-fried", "avatar-cocina", _localizer["Cocina"]),
        RolEmpleado.Auxiliar => ("bi-person-heart", "avatar-auxiliar", _localizer["Auxiliar"]),
        RolEmpleado.Direccion => ("bi-graph-up-arrow", "avatar-direccion", _localizer["Dirección"]),
        RolEmpleado.Enfermeria => ("bi-heart-pulse-fill", "avatar-enfermeria", _localizer["Enfermería"]),
        RolEmpleado.Profesional => ("bi-clipboard2-pulse-fill", "avatar-profesional", _localizer["Profesional"]),
        RolEmpleado.Limpieza => ("bi-stars", "avatar-limpieza", _localizer["Limpieza"]),
        RolEmpleado.DirectorOrganizacion => ("bi-diagram-3-fill", "avatar-directororg", _localizer["Director de Organización"]),
        _ => ("bi-person-fill", "avatar-admin", "")
    };
}
