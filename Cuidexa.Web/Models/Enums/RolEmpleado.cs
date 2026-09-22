namespace Cuidexa.Web.Models.Enums;

// Roles incluidos en este MVP. Al añadir un rol nuevo (ej. Enfermeria),
// solo hace falta añadirlo aquí y crear su controlador/vista correspondiente.
public enum RolEmpleado
{
    Admin,
    Cocina,
    Auxiliar,
    Direccion,
    Enfermeria,
    Profesional,
    Limpieza,

    // Ve y gestiona todos los Centros de su misma Organizacion (Fase 9) —
    // a diferencia del resto de roles, que están atados a un único Centro.
    // Nombre elegido para no confundirse con "Direccion" (rol operativo de
    // un solo centro) en [Authorize(Roles=...)] ni en switches por rol.
    DirectorOrganizacion
}
