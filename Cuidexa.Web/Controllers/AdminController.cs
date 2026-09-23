using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

// Panel de aterrizaje de Admin tras el login: antes caía directo en
// Residentes/Index (una tabla), sin visión de conjunto. Es de solo lectura
// (los accesos rápidos llevan a los controladores que ya gestionan cada
// cosa) — no duplica ninguna acción de escritura existente.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly CuidexaDbContext _db;
    private readonly IEventoDistribucionService _eventos;
    private readonly ITurnoService _turnos;
    private readonly ICentroService _centro;
    private readonly IIncidenciaService _incidencias;

    public AdminController(CuidexaDbContext db, IEventoDistribucionService eventos, ITurnoService turnos, ICentroService centro, IIncidenciaService incidencias)
    {
        _db = db;
        _eventos = eventos;
        _turnos = turnos;
        _centro = centro;
        _incidencias = incidencias;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalResidentes = await _db.Residentes.CountAsync(r => r.Estado == EstadoResidente.Activo);
        ViewBag.TotalPersonal = await _db.Empleados.CountAsync(e => e.Activo);

        // Personal realmente en turno hoy (por defecto o por excepción) — no
        // cuenta a quien le cedieron su turno ese día.
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var turnosHoy = await _turnos.ObtenerTurnosEfectivosAsync(hoy, hoy, null);
        ViewBag.TurnosHoy = turnosHoy.Count(t => !t.Cedido);

        // Avisos dirigidos específicamente a Admin (los manuales que un
        // operativo marca como destino obligatorio) — mismo criterio que el
        // resto de paneles: "mis" avisos pendientes, no el total del centro
        // (eso ya lo cubre el desglose por departamento de Dirección).
        ViewBag.AvisosPendientes = (await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Admin, EmpleadoIdActual ?? 0)).Count(e => !e.Leido);
        ViewBag.CambiosPendientes = await _db.CambiosTurno.CountAsync(c => c.Estado == EstadoCambioTurno.Pendiente);

        ViewBag.ActividadReciente = await _db.AuditLogs
            .Include(a => a.Empleado)
            .OrderByDescending(a => a.FechaHora)
            .Take(6)
            .ToListAsync();

        var distribucion = await _db.Residentes
            .Where(r => r.Estado == EstadoResidente.Activo)
            .GroupBy(r => r.Movilidad)
            .Select(g => new { Movilidad = g.Key, Cantidad = g.Count() })
            .ToListAsync();
        ViewBag.Distribucion = distribucion.ToDictionary(d => d.Movilidad, d => d.Cantidad);

        return View();
    }

    // Admin no crea avisos (eso lo hacen los roles operativos) — solo los
    // recibe, ya que es destino obligatorio de todo aviso manual.
    public async Task<IActionResult> Avisos()
    {
        return View(await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Admin, EmpleadoIdActual ?? 0));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarLeido(int id)
    {
        await _eventos.MarcarLeidoAsync(id);
        return RedirectToAction("Avisos");
    }

    // Solo los datos físicos del propio centro (nombre, contacto,
    // responsable) — la marca compartida (nombre de marca, logo, color) vive
    // en Organizacion y se edita desde /Organizacion/Marca, exclusivo de
    // DirectorOrganizacion (ver OrganizacionController).
    public async Task<IActionResult> Centro()
    {
        return View(await _centro.ObtenerAsync());
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarCentro(string nombre, string? direccion, string? telefono, string? email,
        string? responsableNombre, string? responsableCargo)
    {
        try
        {
            await _centro.ActualizarContactoAsync(nombre, direccion, telefono, email, responsableNombre, responsableCargo, EmpleadoIdActual);
            TempData["Mensaje"] = "Datos del centro actualizados.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Centro");
    }

    // Admin no reporta incidencias (igual que con Avisos) — solo las recibe
    // y resuelve las que le tocan (Personal/Instalaciones, más oversight de
    // cualquier otro tipo — ver IncidenciaService.RolResponsable).
    public async Task<IActionResult> Incidencias()
    {
        return View("~/Views/Shared/Incidencias.cshtml", new IncidenciasViewModel
        {
            Incidencias = await _incidencias.ObtenerParaRolAsync(RolEmpleado.Admin),
            RutaBase = "Admin",
            PuedeCrear = false
        });
    }

    [HttpPost]
    public async Task<IActionResult> ResolverIncidencia(int id, Models.Enums.EstadoIncidencia estado, string? notasResolucion)
    {
        try
        {
            await _incidencias.CambiarEstadoAsync(id, estado, notasResolucion, EmpleadoIdActual);
            TempData["Mensaje"] = "Incidencia actualizada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Incidencias");
    }
}
