using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Admin-only: alta/edición de las tablets compartidas por departamento
// (Fase 6). La sesión de la propia tablet vive en TabletController, con su
// propio esquema de cookie — este controlador no la usa ni la conoce.
[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class DispositivosController : Controller
{
    private static readonly RolEmpleado[] RolesPermitidos =
        { RolEmpleado.Cocina, RolEmpleado.Enfermeria, RolEmpleado.Auxiliar };

    private readonly ICuentaDispositivoService _dispositivos;
    private readonly ICentroService _centro;
    private readonly ITenantContext _tenant;

    public DispositivosController(ICuentaDispositivoService dispositivos, ICentroService centro, ITenantContext tenant)
    {
        _dispositivos = dispositivos;
        _centro = centro;
        _tenant = tenant;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        ViewBag.MostrarCentro = _tenant.AccesoOrganizacionCompleto;
        return View(await _dispositivos.ObtenerTodasAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = RolesPermitidos;
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(new CuentaDispositivoCreateDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CuentaDispositivoCreateDto dto, int? centroId)
    {
        try
        {
            await _dispositivos.CrearAsync(dto, EmpleadoIdActual, centroId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Roles = RolesPermitidos;
            ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
            return View(dto);
        }

        TempData["Mensaje"] = "Tablet creada.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cuenta = await _dispositivos.ObtenerPorIdAsync(id);
        if (cuenta is null) return NotFound();

        var dto = new CuentaDispositivoEditDto { Nombre = cuenta.Nombre, Rol = cuenta.Rol };
        ViewBag.CuentaId = id;
        ViewBag.Roles = RolesPermitidos;
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CuentaDispositivoEditDto dto)
    {
        try
        {
            await _dispositivos.ActualizarAsync(id, dto, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.CuentaId = id;
            ViewBag.Roles = RolesPermitidos;
            return View(dto);
        }

        TempData["Mensaje"] = "Tablet actualizada.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        try
        {
            await _dispositivos.CambiarEstadoAsync(id, activo, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}
