using Cuidexa.Web.Data;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class EmpleadosController : Controller
{
    private readonly IEmpleadoService _empleados;
    private readonly IPlantillaTurnoService _plantillas;
    private readonly ICentroService _centro;
    private readonly ITenantContext _tenant;
    private readonly CuidexaDbContext _db;

    public EmpleadosController(IEmpleadoService empleados, IPlantillaTurnoService plantillas, ICentroService centro,
        ITenantContext tenant, CuidexaDbContext db)
    {
        _empleados = empleados;
        _plantillas = plantillas;
        _centro = centro;
        _tenant = tenant;
        _db = db;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        var lista = await _empleados.ObtenerTodosAsync();
        ViewBag.EmpleadoIdActual = EmpleadoIdActual;
        ViewBag.MostrarCentro = _tenant.AccesoOrganizacionCompleto;
        return View(lista);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Especialidades = await _db.Especialidades.ToListAsync();
        ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(new EmpleadoCreateDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmpleadoCreateDto dto, int? centroId)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _empleados.CrearAsync(dto, EmpleadoIdActual, centroId);
                TempData["Mensaje"] = "Usuario creado.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        ViewBag.Especialidades = await _db.Especialidades.ToListAsync();
        ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await _empleados.ObtenerPorIdAsync(id);
        if (empleado is null) return NotFound();

        var dto = new EmpleadoEditDto
        {
            Nombre = empleado.Nombre,
            Email = empleado.Email,
            Rol = empleado.Rol,
            EspecialidadId = empleado.EspecialidadId,
            PlantillaTurnoDefectoId = empleado.PlantillaTurnoDefectoId
        };
        ViewBag.EmpleadoId = id;
        ViewBag.Especialidades = await _db.Especialidades.ToListAsync();
        ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EmpleadoEditDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EmpleadoId = id;
            ViewBag.Especialidades = await _db.Especialidades.ToListAsync();
            ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
            return View(dto);
        }

        try
        {
            await _empleados.ActualizarAsync(id, dto, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.EmpleadoId = id;
            ViewBag.Especialidades = await _db.Especialidades.ToListAsync();
            ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
            return View(dto);
        }

        TempData["Mensaje"] = "Usuario actualizado.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        try
        {
            await _empleados.CambiarEstadoAsync(id, activo, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}
