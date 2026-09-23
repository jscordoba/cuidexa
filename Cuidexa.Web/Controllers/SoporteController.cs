using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Canal de soporte, común a cualquier rol de Empleado (no duplicado por rol
// como Avisos/Incidencias — no necesita lógica distinta por departamento).
[Authorize]
public class SoporteController : Controller
{
    private readonly ISoporteService _soporte;

    public SoporteController(ISoporteService soporte)
    {
        _soporte = soporte;
    }

    private int? EmpleadoIdActual() =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        return View(await _soporte.ObtenerParaCentroActualAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(string titulo, string descripcion)
    {
        try
        {
            await _soporte.CrearTicketAsync(titulo, descripcion, EmpleadoIdActual());
            TempData["Mensaje"] = "Tu incidencia de soporte se ha enviado. Te responderemos aquí mismo.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Index");
    }
}
