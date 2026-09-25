using System.Security.Claims;
using Cuidexa.Web.Data;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

// Portal de familiares (Fase 10 — comercialización, bloque 5): acceso de
// solo lectura a un único Residente. Esquema de cookie propio ("Familiar",
// ver Program.cs) — un familiar no tiene CentroId/OrganizacionId, y su
// alcance real (un Residente concreto) es más estrecho que el de cualquier
// rol operativo, así que nunca debe poder satisfacer un
// [Authorize(Roles = "...")] de los controladores de personal.
[Authorize(AuthenticationSchemes = EsquemaFamiliar)]
public class FamiliarController : Controller
{
    public const string EsquemaFamiliar = "Familiar";

    private readonly CuidexaDbContext _db;
    private readonly IFamiliarService _familiares;
    private readonly IPushNotificationService _push;

    public FamiliarController(CuidexaDbContext db, IFamiliarService familiares, IPushNotificationService push)
    {
        _db = db;
        _familiares = familiares;
        _push = push;
    }

    private int FamiliarIdActual() => int.Parse(User.FindFirst("FamiliarId")!.Value);

    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var familiar = await _db.Familiares.FirstOrDefaultAsync(f => f.Email == email && f.Activo);
        if (familiar is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        if (BloqueoCuenta.EstaBloqueada(familiar))
        {
            ModelState.AddModelError("", $"Demasiados intentos fallidos. Inténtalo de nuevo en {BloqueoCuenta.MinutosRestantes(familiar)} minuto(s).");
            return View();
        }

        if (!PasswordHasher.Verify(password, familiar.PasswordHash))
        {
            BloqueoCuenta.RegistrarFallo(familiar);
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        BloqueoCuenta.Desbloquear(familiar);
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, familiar.Nombre),
            new("FamiliarId", familiar.Id.ToString()),
            new("ResidenteId", familiar.ResidenteId.ToString())
        };

        var identity = new ClaimsIdentity(claims, EsquemaFamiliar);
        await HttpContext.SignInAsync(EsquemaFamiliar, new ClaimsPrincipal(identity));

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(EsquemaFamiliar);
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Index()
    {
        var residenteId = int.Parse(User.FindFirst("ResidenteId")!.Value);

        // Sin filtro de tenant (el familiar no tiene Centro de sesión), pero
        // Residente no lleva query filter por sí mismo en este punto salvo
        // el heredado — buscamos directo por Id, que es exactamente el
        // alcance ya validado en el login (ResidenteId viene del propio
        // Familiar, no de un parámetro manipulable).
        var residente = await _db.Residentes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == residenteId);
        if (residente is null)
        {
            return NotFound();
        }

        ViewBag.Residente = residente;
        ViewBag.Documentos = await _familiares.ObtenerDocumentosAsync(residenteId);
        ViewBag.Incidencias = await _familiares.ObtenerIncidenciasResueltasAsync(residenteId);
        return View();
    }

    public class SuscripcionRequest
    {
        public string Endpoint { get; set; } = string.Empty;
        public string P256dh { get; set; } = string.Empty;
        public string Auth { get; set; } = string.Empty;
    }

    public class DesuscripcionRequest
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> SuscribirPush([FromBody] SuscripcionRequest request)
    {
        await _push.SuscribirFamiliarAsync(FamiliarIdActual(), request.Endpoint, request.P256dh, request.Auth);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DesuscribirPush([FromBody] DesuscripcionRequest request)
    {
        await _push.DesuscribirFamiliarAsync(request.Endpoint);
        return Ok();
    }
}
