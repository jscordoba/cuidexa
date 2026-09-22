using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Admin-only: grupos ad-hoc de empleados para dirigir avisos manuales sin
// notificar a todo un departamento (Fase 7). Calca EmpleadosController.
[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class GruposController : Controller
{
    private readonly IGrupoNotificacionService _grupos;
    private readonly IEmpleadoService _empleados;
    private readonly ICentroService _centro;
    private readonly ITenantContext _tenant;

    public GruposController(IGrupoNotificacionService grupos, IEmpleadoService empleados, ICentroService centro, ITenantContext tenant)
    {
        _grupos = grupos;
        _empleados = empleados;
        _centro = centro;
        _tenant = tenant;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        ViewBag.MostrarCentro = _tenant.AccesoOrganizacionCompleto;
        return View(await _grupos.ObtenerTodosAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Empleados = (await _empleados.ObtenerTodosAsync()).Where(e => e.Activo).ToList();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(new GrupoNotificacionDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(GrupoNotificacionDto dto, int? centroId)
    {
        try
        {
            await _grupos.CrearAsync(dto, EmpleadoIdActual, centroId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Empleados = (await _empleados.ObtenerTodosAsync()).Where(e => e.Activo).ToList();
            ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
            return View(dto);
        }

        TempData["Mensaje"] = "Grupo creado.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var grupo = await _grupos.ObtenerPorIdAsync(id);
        if (grupo is null) return NotFound();

        var miembros = await _grupos.ObtenerMiembrosAsync(id);
        var dto = new GrupoNotificacionDto { Nombre = grupo.Nombre, EmpleadoIds = miembros.Select(m => m.Id).ToList() };
        ViewBag.GrupoId = id;
        ViewBag.Empleados = (await _empleados.ObtenerTodosAsync()).Where(e => e.Activo).ToList();
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, GrupoNotificacionDto dto)
    {
        try
        {
            await _grupos.ActualizarAsync(id, dto, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.GrupoId = id;
            ViewBag.Empleados = (await _empleados.ObtenerTodosAsync()).Where(e => e.Activo).ToList();
            return View(dto);
        }

        TempData["Mensaje"] = "Grupo actualizado.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        try
        {
            await _grupos.CambiarEstadoAsync(id, activo, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}
