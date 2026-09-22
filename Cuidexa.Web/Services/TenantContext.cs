using System.Security.Claims;
using Cuidexa.Web.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Cuidexa.Web.Services;

public class TenantContext : ITenantContext
{
    private readonly Lazy<int> _centroId;
    private readonly Lazy<int> _organizacionId;
    private readonly Lazy<bool> _accesoOrganizacionCompleto;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var usuario = httpContextAccessor.HttpContext?.User;

        // Sin claims (login pages, sesión SuperAdmin) → 0, que no coincide con
        // ningún Centro/Organización real: nunca filtra "de más" por accidente.
        _centroId = new Lazy<int>(() =>
            int.TryParse(usuario?.FindFirst("CentroId")?.Value, out var id) ? id : 0);

        _organizacionId = new Lazy<int>(() =>
            int.TryParse(usuario?.FindFirst("OrganizacionId")?.Value, out var id) ? id : 0);

        _accesoOrganizacionCompleto = new Lazy<bool>(() =>
            Enum.TryParse<RolEmpleado>(usuario?.FindFirst(ClaimTypes.Role)?.Value, out var rol)
            && rol == RolEmpleado.DirectorOrganizacion);
    }

    public int CentroId => _centroId.Value;
    public int OrganizacionId => _organizacionId.Value;
    public bool AccesoOrganizacionCompleto => _accesoOrganizacionCompleto.Value;
}
