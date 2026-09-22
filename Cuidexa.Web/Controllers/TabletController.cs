using System.Security.Claims;
using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

// Sesión de tablet compartida (Fase 6) — separada a propósito de
// AccountController/EmpleadoController: usa su propio esquema de cookie
// ("Dispositivo", registrado en Program.cs) para que nunca sea reconocida
// por los [Authorize(Roles = "...")] de los controladores operativos, que
// solo validan contra el esquema por defecto de Empleado. Así "solo
// consulta" queda garantizado a nivel de framework, no por checks sueltos.
public class TabletController : Controller
{
    public const string EsquemaDispositivo = "Dispositivo";

    private readonly ICuentaDispositivoService _dispositivos;
    private readonly CuidexaDbContext _db;

    public TabletController(ICuentaDispositivoService dispositivos, CuidexaDbContext db)
    {
        _dispositivos = dispositivos;
        _db = db;
    }

    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string nombre, string password, string codigoCentro)
    {
        var centro = await _db.Centros.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Codigo == codigoCentro && c.Activo);
        if (centro is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        CuentaDispositivo? cuenta;
        try
        {
            cuenta = await _dispositivos.AutenticarAsync(nombre, password, centro.Id);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }

        if (cuenta is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, cuenta.Nombre),
            new(ClaimTypes.Role, cuenta.Rol.ToString()),
            new("CuentaDispositivoId", cuenta.Id.ToString()),
            new("CentroId", cuenta.CentroId.ToString()),
            new("OrganizacionId", centro.OrganizacionId.ToString())
        };

        var identity = new ClaimsIdentity(claims, EsquemaDispositivo);
        await HttpContext.SignInAsync(EsquemaDispositivo, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return RedirectToAction("Dashboard");
    }

    [Authorize(AuthenticationSchemes = EsquemaDispositivo)]
    public async Task<IActionResult> Dashboard()
    {
        var rolTexto = User.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<RolEmpleado>(rolTexto, out var rol))
        {
            return RedirectToAction("Login");
        }

        var idTexto = User.FindFirst("CuentaDispositivoId")?.Value;
        var cuenta = int.TryParse(idTexto, out var id) ? await _dispositivos.ObtenerPorIdAsync(id) : null;
        if (cuenta is null || !cuenta.Activo)
        {
            await HttpContext.SignOutAsync(EsquemaDispositivo);
            return RedirectToAction("Login");
        }

        return View(await _dispositivos.ObtenerResumenAsync(cuenta));
    }

    [Authorize(AuthenticationSchemes = EsquemaDispositivo)]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(EsquemaDispositivo);
        return RedirectToAction("Login");
    }
}
