namespace Cuidexa.Web.Services;

// Resuelve el Centro/Organización del request actual a partir de las claims
// de sesión (ver AccountController/TabletController) — lo usa CuidexaDbContext
// para los filtros globales de aislamiento y los servicios que crean filas
// nuevas (para fijar CentroId/OrganizacionId explícitamente).
public interface ITenantContext
{
    int CentroId { get; }
    int OrganizacionId { get; }

    // Solo true para RolEmpleado.DirectorOrganizacion — ve todos los Centros
    // de su Organizacion, no solo el suyo propio.
    bool AccesoOrganizacionCompleto { get; }
}
