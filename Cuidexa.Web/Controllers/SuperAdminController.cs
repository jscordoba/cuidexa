using System.Security.Claims;
using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

// Provisión manual de Organizaciones/Centros nuevos (Fase 9) — nunca
// autoservicio público. Esquema de cookie propio (ver Program.cs), separado
// de Empleado/Dispositivo: SuperAdmin no tiene CentroId/OrganizacionId, así
// que nunca debe poder satisfacer un [Authorize(Roles = "...")] operativo.
[Authorize(AuthenticationSchemes = EsquemaSuperAdmin)]
public class SuperAdminController : Controller
{
    public const string EsquemaSuperAdmin = "SuperAdmin";

    private readonly CuidexaDbContext _db;
    private readonly IEmpleadoService _empleados;
    private readonly IOrganizacionService _organizacion;
    private readonly ISoporteService _soporte;

    public SuperAdminController(CuidexaDbContext db, IEmpleadoService empleados, IOrganizacionService organizacion, ISoporteService soporte)
    {
        _db = db;
        _empleados = empleados;
        _organizacion = organizacion;
        _soporte = soporte;
    }

    private int SuperAdminIdActual() => int.Parse(User.FindFirst("SuperAdminId")!.Value);

    // Sin comprobación de "ya autenticado": HttpContext.User refleja el
    // esquema por defecto (Empleado), no el de SuperAdmin, así que aquí no
    // hay forma barata de saberlo sin autenticar explícitamente contra este
    // esquema — mismo criterio que ya sigue TabletController.Login().
    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var cuenta = await _db.SuperAdmins.FirstOrDefaultAsync(s => s.Email == email && s.Activo);
        if (cuenta is null)
        {
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        if (BloqueoCuenta.EstaBloqueada(cuenta))
        {
            ModelState.AddModelError("", $"Demasiados intentos fallidos. Inténtalo de nuevo en {BloqueoCuenta.MinutosRestantes(cuenta)} minuto(s).");
            return View();
        }

        if (!PasswordHasher.Verify(password, cuenta.PasswordHash))
        {
            BloqueoCuenta.RegistrarFallo(cuenta);
            await _db.SaveChangesAsync();
            ModelState.AddModelError("", "Credenciales incorrectas.");
            return View();
        }

        BloqueoCuenta.Desbloquear(cuenta);
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, cuenta.Nombre),
            new(ClaimTypes.Role, "SuperAdmin"),
            new("SuperAdminId", cuenta.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, EsquemaSuperAdmin);
        await HttpContext.SignInAsync(EsquemaSuperAdmin, new ClaimsPrincipal(identity));

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(EsquemaSuperAdmin);
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Index()
    {
        var organizaciones = await _db.Organizaciones
            .Include(o => o.Centros)
            .OrderBy(o => o.Nombre)
            .ToListAsync();
        return View(organizaciones);
    }

    public IActionResult CrearOrganizacion() => View();

    [HttpPost]
    public async Task<IActionResult> CrearOrganizacion(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError("", "El nombre de la organización es obligatorio.");
            return View();
        }

        var organizacion = new Organizacion { Nombre = nombre };
        _db.Organizaciones.Add(organizacion);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Organización '{organizacion.Nombre}' creada. Ahora añade su primer centro.";
        return RedirectToAction("CrearCentro", new { organizacionId = organizacion.Id });
    }

    public async Task<IActionResult> CrearCentro(int organizacionId)
    {
        var organizacion = await _db.Organizaciones.FindAsync(organizacionId);
        if (organizacion is null) return NotFound();

        ViewBag.Organizacion = organizacion;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearCentro(int organizacionId, string nombre, string codigo, string? direccion,
        string? telefono, string? email, string? responsableNombre, string? responsableCargo)
    {
        var organizacion = await _db.Organizaciones.FindAsync(organizacionId);
        if (organizacion is null) return NotFound();

        codigo = codigo?.Trim().ToUpperInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
        {
            ModelState.AddModelError("", "El nombre y el código del centro son obligatorios.");
            ViewBag.Organizacion = organizacion;
            return View();
        }

        if (await _db.Centros.AnyAsync(c => c.Codigo == codigo))
        {
            ModelState.AddModelError("", "Ya existe un centro con ese código.");
            ViewBag.Organizacion = organizacion;
            return View();
        }

        var centro = new Centro
        {
            OrganizacionId = organizacionId,
            Nombre = nombre,
            Codigo = codigo,
            Direccion = direccion,
            Telefono = telefono,
            Email = email,
            ResponsableNombre = responsableNombre,
            ResponsableCargo = responsableCargo
        };

        _db.Centros.Add(centro);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Centro '{centro.Nombre}' (código {centro.Codigo}) creado. Ahora da de alta su primer Admin.";
        return RedirectToAction("CrearAdminInicial", new { centroId = centro.Id });
    }

    public async Task<IActionResult> CrearAdminInicial(int centroId)
    {
        var centro = await _db.Centros.Include(c => c.Organizacion).FirstOrDefaultAsync(c => c.Id == centroId);
        if (centro is null) return NotFound();

        ViewBag.Centro = centro;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearAdminInicial(int centroId, string nombre, string email, string password)
    {
        var centro = await _db.Centros.Include(c => c.Organizacion).FirstOrDefaultAsync(c => c.Id == centroId);
        if (centro is null) return NotFound();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            ModelState.AddModelError("", "Rellena nombre, email y una contraseña de al menos 8 caracteres.");
            ViewBag.Centro = centro;
            return View();
        }

        try
        {
            await _empleados.CrearAdminInicialAsync(centroId, nombre, email, password);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Centro = centro;
            return View();
        }

        TempData["Mensaje"] = $"Admin '{nombre}' creado para el centro '{centro.Nombre}' (código {centro.Codigo}). Ya puede iniciar sesión.";
        return RedirectToAction("Index");
    }

    // Marca de una Organización desde el panel de SuperAdmin — cubre el
    // hueco de una organización sin ningún DirectorOrganizacion todavía
    // (recién provisionada, solo con su primer Admin) que de otro modo se
    // queda con la marca genérica hasta que alguien sea ascendido. Vale
    // igual para una organización de un único centro que para una con
    // varios — la marca es de la Organización, no depende de cuántos tenga.
    public async Task<IActionResult> Marca(int organizacionId)
    {
        Organizacion organizacion;
        try
        {
            organizacion = await _organizacion.ObtenerPorIdAsync(organizacionId);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        return View(organizacion);
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarMarca(int organizacionId, string nombre, string? colorAcento)
    {
        try
        {
            await _organizacion.ActualizarMarcaComoSuperAdminAsync(organizacionId, nombre, colorAcento);
            TempData["Mensaje"] = "Marca de la organización actualizada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Marca", new { organizacionId });
    }

    [HttpPost]
    public async Task<IActionResult> SubirLogo(int organizacionId, IFormFile? logo)
    {
        try
        {
            await _organizacion.ActualizarLogoComoSuperAdminAsync(organizacionId, logo);
            TempData["Mensaje"] = "Logo actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Marca", new { organizacionId });
    }

    // Canal de soporte (COMERCIALIZACION.md, bloque 3): SuperAdmin ve los
    // tickets de todos los centros/organizaciones, no solo del suyo (no
    // tiene "el suyo" — opera por encima del modelo de tenant).
    public async Task<IActionResult> Soporte()
    {
        return View(await _soporte.ObtenerTodosAsync());
    }

    [HttpPost]
    public async Task<IActionResult> ResponderTicket(int ticketId, string respuesta)
    {
        try
        {
            await _soporte.ResponderAsync(ticketId, respuesta, SuperAdminIdActual());
            TempData["Mensaje"] = "Respuesta enviada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Soporte");
    }
}
