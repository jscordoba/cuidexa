using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Suscripción/desuscripción de push del propio empleado logueado — nunca de
// otro. [Authorize] sin Roles: cualquier empleado autenticado puede activar
// notificaciones para sí mismo (no las tablets compartidas, ver Program.cs:
// el esquema por defecto es el único que autentica aquí).
[Authorize]
public class PushController : Controller
{
    private readonly IPushNotificationService _push;

    public PushController(IPushNotificationService push)
    {
        _push = push;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public class SuscripcionRequest
    {
        public string Endpoint { get; set; } = string.Empty;
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Suscribir([FromBody] SuscripcionRequest request)
    {
        if (EmpleadoIdActual is null) return Unauthorized();

        await _push.SuscribirAsync(EmpleadoIdActual.Value, request.Endpoint, request.P256dh, request.Auth);
        return Ok();
    }

    public class DesuscripcionRequest
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Desuscribir([FromBody] DesuscripcionRequest request)
    {
        await _push.DesuscribirAsync(request.Endpoint);
        return Ok();
    }
}
