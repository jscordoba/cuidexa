using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Marca compartida por todos los Centros de la organización (Fase 9) —
// separado de AdminController a propósito: mezclar aquí un segundo nivel de
// [Authorize] específico para estas acciones (además del "Admin" de
// AdminController) obligaría a pelear con cómo ASP.NET Core intersecta
// [Authorize] de controlador y de acción — un controlador propio, solo para
// DirectorOrganizacion, lo evita por completo.
[Authorize(Roles = "DirectorOrganizacion")]
public class OrganizacionController : Controller
{
    private readonly IOrganizacionService _organizacion;

    public OrganizacionController(IOrganizacionService organizacion)
    {
        _organizacion = organizacion;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Marca()
    {
        return View(await _organizacion.ObtenerAsync());
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarMarca(string nombre, string? colorAcento)
    {
        try
        {
            await _organizacion.ActualizarMarcaAsync(nombre, colorAcento, EmpleadoIdActual);
            TempData["Mensaje"] = "Marca de la organización actualizada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Marca");
    }

    [HttpPost]
    public async Task<IActionResult> SubirLogo(IFormFile? logo)
    {
        try
        {
            await _organizacion.ActualizarLogoAsync(logo, EmpleadoIdActual);
            TempData["Mensaje"] = "Logo actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Marca");
    }
}
