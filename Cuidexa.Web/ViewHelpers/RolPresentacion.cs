using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.ViewHelpers;

// Icono, color y etiqueta de cada rol para la interfaz (avatar de saludo,
// barra lateral de escritorio...) — en un único sitio para que las distintas
// vistas que necesitan "pintar" un rol nunca se desincronicen entre sí.
public static class RolPresentacion
{
    public static (string Icono, string ClaseColor, string Etiqueta) Obtener(RolEmpleado rol) => rol switch
    {
        RolEmpleado.Admin => ("bi-shield-lock-fill", "avatar-admin", "Administración"),
        RolEmpleado.Cocina => ("bi-egg-fried", "avatar-cocina", "Cocina"),
        RolEmpleado.Auxiliar => ("bi-person-heart", "avatar-auxiliar", "Auxiliar"),
        RolEmpleado.Direccion => ("bi-graph-up-arrow", "avatar-direccion", "Dirección"),
        RolEmpleado.Enfermeria => ("bi-heart-pulse-fill", "avatar-enfermeria", "Enfermería"),
        RolEmpleado.Profesional => ("bi-clipboard2-pulse-fill", "avatar-profesional", "Profesional"),
        RolEmpleado.Limpieza => ("bi-stars", "avatar-limpieza", "Limpieza"),
        RolEmpleado.DirectorOrganizacion => ("bi-diagram-3-fill", "avatar-directororg", "Director de Organización"),
        _ => ("bi-person-fill", "avatar-admin", "")
    };
}
