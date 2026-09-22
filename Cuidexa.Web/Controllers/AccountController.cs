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

public class AccountController : Controller
{
    private readonly CuidexaDbContext _db;

    public AccountController(CuidexaDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        // Si ya hay sesión (llegas aquí por "/" o por el enlace "Cuidexa" del
        // menú estando logueado), no tiene sentido volver a mostrar el
        // formulario de login — se manda directo a tu panel.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectParaRol(User);
        }

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string email, string password, string codigoCentro)
    {
        // El Centro no lleva filtro global, pero .IgnoreQueryFilters() aquí
        // documenta que esta consulta se hace deliberadamente sin tenant
        // conocido todavía (es justo lo que este paso resuelve).
        var centro = await _db.Centros.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Codigo == codigoCentro && c.Activo);

        // Mismo mensaje genérico si falla el código de centro o el email/
        // contraseña: no revelar cuál de los tres fue el que no coincidió.
        if (centro is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        // Imprescindible IgnoreQueryFilters(): en este punto de la petición
        // ITenantContext todavía no tiene claims (nadie ha iniciado sesión),
        // así que el filtro global evaluaría CentroId == 0 y no encontraría
        // nunca nada — el alcance real lo da el .CentroId == centro.Id de abajo.
        var empleado = await _db.Empleados.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Email == email && e.CentroId == centro.Id && e.Activo);

        if (empleado is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        // Bloqueo por fuerza bruta (por CUENTA, complementa el límite por IP
        // de Program.cs) — se comprueba antes de verificar la contraseña.
        if (BloqueoCuenta.EstaBloqueada(empleado))
        {
            ModelState.AddModelError("", $"Demasiados intentos fallidos. Inténtalo de nuevo en {BloqueoCuenta.MinutosRestantes(empleado)} minuto(s), o pide a un Admin que te restablezca la contraseña.");
            return View();
        }

        if (!PasswordHasher.Verify(password, empleado.PasswordHash))
        {
            BloqueoCuenta.RegistrarFallo(empleado);
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        BloqueoCuenta.Desbloquear(empleado);
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, empleado.Nombre),
            new(ClaimTypes.Role, empleado.Rol.ToString()),
            new("EmpleadoId", empleado.Id.ToString()),
            new("CentroId", empleado.CentroId.ToString()),
            new("OrganizacionId", centro.OrganizacionId.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectParaRol(empleado.Rol);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private IActionResult RedirectParaRol(ClaimsPrincipal usuario)
    {
        var rolTexto = usuario.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<Models.Enums.RolEmpleado>(rolTexto, out var rol)
            ? RedirectParaRol(rol)
            : RedirectToAction("Login");
    }

    // Cada rol aterriza en su propia pantalla principal — un único punto
    // (usado tanto tras el login como al volver con sesión ya activa) para
    // no mantener este mapeo duplicado en dos sitios.
    private IActionResult RedirectParaRol(Models.Enums.RolEmpleado rol) => rol switch
    {
        Models.Enums.RolEmpleado.Admin => RedirectToAction("Index", "Admin"),
        Models.Enums.RolEmpleado.Cocina => RedirectToAction("Index", "Cocina"),
        Models.Enums.RolEmpleado.Auxiliar => RedirectToAction("Index", "Auxiliares"),
        Models.Enums.RolEmpleado.Direccion => RedirectToAction("Index", "Direccion"),
        Models.Enums.RolEmpleado.Enfermeria => RedirectToAction("Index", "Enfermeria"),
        Models.Enums.RolEmpleado.Profesional => RedirectToAction("Index", "Profesionales"),
        Models.Enums.RolEmpleado.Limpieza => RedirectToAction("Index", "Limpieza"),
        // Sin dashboard propio (ve datos combinados de su organización a
        // través de las mismas pantallas de Admin: Residentes, Empleados,
        // Turnos, Informes) — aterriza en Residentes, el mismo punto de
        // partida operativo que un Admin, con "Marca" accesible desde el nav.
        Models.Enums.RolEmpleado.DirectorOrganizacion => RedirectToAction("Index", "Residentes"),
        _ => RedirectToAction("Login")
    };
}
