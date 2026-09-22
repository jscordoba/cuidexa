using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class TurnosController : Controller
{
    private readonly ITurnoService _turnos;
    private readonly IVacacionService _vacaciones;
    private readonly IPlantillaTurnoService _plantillas;
    private readonly IAuditService _auditoria;
    private readonly ICentroService _centro;
    private readonly ITenantContext _tenant;
    private readonly IFestivosApiService _festivosApi;
    private readonly CuidexaDbContext _db;

    public TurnosController(ITurnoService turnos, IVacacionService vacaciones, IPlantillaTurnoService plantillas,
        IAuditService auditoria, ICentroService centro, ITenantContext tenant, IFestivosApiService festivosApi, CuidexaDbContext db)
    {
        _turnos = turnos;
        _vacaciones = vacaciones;
        _plantillas = plantillas;
        _auditoria = auditoria;
        _centro = centro;
        _tenant = tenant;
        _festivosApi = festivosApi;
        _db = db;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        ViewBag.CambiosPendientes = (await _turnos.ObtenerCambiosPendientesAsync()).Count;
        ViewBag.MostrarCentro = _tenant.AccesoOrganizacionCompleto;
        return View(await _turnos.ObtenerTodosAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Empleados = await _db.Empleados.Include(e => e.Centro).Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
        ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas, int? centroId)
    {
        try
        {
            await _turnos.CrearTurnoAsync(rol, empleadoId, nombreExterno, fechaInicio, fechaFin, plantillaTurnoId, notas, EmpleadoIdActual, centroId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
            ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
            ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
            return View();
        }

        TempData["Mensaje"] = "Turno creado.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Editar(int id)
    {
        var turno = await _turnos.ObtenerPorIdAsync(id);
        if (turno is null) return NotFound();

        ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
        ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
        return View(turno);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, RolEmpleado rol, int? empleadoId, string? nombreExterno, DateTime fechaInicio, DateTime fechaFin, int? plantillaTurnoId, string? notas)
    {
        try
        {
            await _turnos.EditarTurnoAsync(id, rol, empleadoId, nombreExterno, fechaInicio, fechaFin, plantillaTurnoId, notas, EmpleadoIdActual);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
            ViewBag.Plantillas = await _plantillas.ObtenerActivasAsync();
            return View(await _turnos.ObtenerPorIdAsync(id));
        }

        TempData["Mensaje"] = "Turno actualizado.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Historial(int turnoId)
    {
        var turno = await _turnos.ObtenerPorIdAsync(turnoId);
        if (turno is null) return NotFound();

        ViewBag.Turno = turno;
        return View(await _auditoria.ObtenerPorEntidadAsync("Turno", turnoId));
    }

    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _turnos.CancelarTurnoAsync(id, EmpleadoIdActual);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Calendario(DateOnly? desde, RolEmpleado? rol)
    {
        // Semana que empieza en lunes, igual criterio que cualquier calendario
        // laboral en España — evita que "hoy" caiga en mitad de la rejilla.
        var hoy = desde ?? DateOnly.FromDateTime(DateTime.Today);
        var diasDesdeLunes = ((int)hoy.DayOfWeek + 6) % 7;
        var inicioSemana = hoy.AddDays(-diasDesdeLunes);
        var finSemana = inicioSemana.AddDays(6);

        var turnos = await _turnos.ObtenerTurnosEfectivosAsync(inicioSemana, finSemana, rol);

        ViewBag.InicioSemana = inicioSemana;
        ViewBag.FinSemana = finSemana;
        ViewBag.RolFiltro = rol;
        ViewBag.Empleados = await _db.Empleados
            .Where(e => e.Activo && (rol == null || e.Rol == rol))
            .OrderBy(e => e.Rol).ThenBy(e => e.Nombre)
            .ToListAsync();
        return View(turnos);
    }

    public async Task<IActionResult> Cambios()
    {
        var cambios = await _turnos.ObtenerCambiosPendientesAsync();

        var candidatosPorRol = new Dictionary<RolEmpleado, List<Empleado>>();
        foreach (var rol in cambios.Select(c => c.Rol).Distinct())
        {
            candidatosPorRol[rol] = await _turnos.ObtenerCompanerosAsync(rol, 0);
        }
        ViewBag.CandidatosPorRol = candidatosPorRol;

        return View(cambios);
    }

    [HttpPost]
    public async Task<IActionResult> AprobarCambio(int id, int empleadoSustitutoId)
    {
        try
        {
            await _turnos.AprobarCambioAsync(id, empleadoSustitutoId, EmpleadoIdActual);
            TempData["Mensaje"] = "Cambio de turno aprobado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Cambios");
    }

    [HttpPost]
    public async Task<IActionResult> RechazarCambio(int id)
    {
        await _turnos.RechazarCambioAsync(id, EmpleadoIdActual);
        TempData["Mensaje"] = "Cambio de turno rechazado.";
        return RedirectToAction("Cambios");
    }

    // --- Plantillas de turno (franjas horarias reutilizables) ---

    public async Task<IActionResult> Plantillas()
    {
        return View(await _plantillas.ObtenerTodasAsync());
    }

    public IActionResult CrearPlantilla() => View();

    [HttpPost]
    public async Task<IActionResult> CrearPlantilla(string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia)
    {
        await _plantillas.CrearAsync(nombre, horaInicio, horaFin, tipoDia);
        TempData["Mensaje"] = "Plantilla creada.";
        return RedirectToAction("Plantillas");
    }

    public async Task<IActionResult> EditarPlantilla(int id)
    {
        var plantilla = await _plantillas.ObtenerPorIdAsync(id);
        if (plantilla is null) return NotFound();
        return View(plantilla);
    }

    [HttpPost]
    public async Task<IActionResult> EditarPlantilla(int id, string nombre, TimeOnly horaInicio, TimeOnly horaFin, TipoDia tipoDia)
    {
        await _plantillas.EditarAsync(id, nombre, horaInicio, horaFin, tipoDia);
        TempData["Mensaje"] = "Plantilla actualizada.";
        return RedirectToAction("Plantillas");
    }

    [HttpPost]
    public async Task<IActionResult> CambiarActivaPlantilla(int id, bool activo)
    {
        await _plantillas.CambiarActivaAsync(id, activo);
        return RedirectToAction("Plantillas");
    }

    // --- Vacaciones ---

    public async Task<IActionResult> Vacaciones()
    {
        var todas = await _vacaciones.ObtenerTodasAsync();
        var festivos = await _vacaciones.ObtenerFechasFeriadosAsync();

        ViewBag.Pendientes = await _vacaciones.ObtenerPendientesAsync();
        ViewBag.Resumen = await _vacaciones.ObtenerResumenAsync();
        ViewBag.Feriados = await _vacaciones.ObtenerFeriadosAsync();
        ViewBag.Centro = await _centro.ObtenerAsync();
        ViewBag.DiasPorVacacion = todas.ToDictionary(
            v => v.Id,
            v => CalculoDiasVacaciones.Contar(v.FechaInicio, v.FechaFin, v.IncluyeFinesSemanaYFestivos, festivos));
        return View(todas);
    }

    [HttpPost]
    public async Task<IActionResult> ActualizarComunidad(ComunidadAutonoma? comunidad)
    {
        await _centro.ActualizarComunidadAsync(comunidad, EmpleadoIdActual);
        TempData["Mensaje"] = "Comunidad autónoma del centro actualizada.";
        return RedirectToAction("Vacaciones");
    }

    [HttpPost]
    public async Task<IActionResult> ImportarFestivos(int anio)
    {
        try
        {
            var centro = await _centro.ObtenerAsync();
            if (centro.ComunidadAutonoma is null)
            {
                throw new InvalidOperationException("Configura primero la comunidad autónoma del centro.");
            }

            var festivos = await _festivosApi.ObtenerFestivosAsync(anio, centro.ComunidadAutonoma.Value);
            var (importados, omitidos) = await _vacaciones.ImportarFeriadosAsync(festivos, EmpleadoIdActual);

            TempData["Mensaje"] = omitidos > 0
                ? $"Importados {importados} festivos de {anio} ({omitidos} ya existían)."
                : $"Importados {importados} festivos de {anio}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Vacaciones");
    }

    public async Task<IActionResult> RegistrarVacacion()
    {
        ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo).OrderBy(e => e.Nombre).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarVacacion(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin, string? notas, bool incluyeFinesSemanaYFestivos)
    {
        try
        {
            await _vacaciones.RegistrarAsync(empleadoId, fechaInicio, fechaFin, notas, incluyeFinesSemanaYFestivos, EmpleadoIdActual);
            TempData["Mensaje"] = "Vacaciones registradas.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Vacaciones");
    }

    [HttpPost]
    public async Task<IActionResult> AprobarVacacion(int id, bool incluyeFinesSemanaYFestivos)
    {
        await _vacaciones.AprobarAsync(id, incluyeFinesSemanaYFestivos, EmpleadoIdActual);
        TempData["Mensaje"] = "Vacaciones aprobadas.";
        return RedirectToAction("Vacaciones");
    }

    [HttpPost]
    public async Task<IActionResult> RechazarVacacion(int id)
    {
        await _vacaciones.RechazarAsync(id, EmpleadoIdActual);
        TempData["Mensaje"] = "Vacaciones rechazadas.";
        return RedirectToAction("Vacaciones");
    }

    [HttpPost]
    public async Task<IActionResult> CrearFeriado(DateOnly fecha, string? nombre)
    {
        try
        {
            await _vacaciones.CrearFeriadoAsync(fecha, nombre);
            TempData["Mensaje"] = "Festivo añadido.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Vacaciones");
    }

    [HttpPost]
    public async Task<IActionResult> EliminarFeriado(int id)
    {
        await _vacaciones.EliminarFeriadoAsync(id);
        TempData["Mensaje"] = "Festivo eliminado.";
        return RedirectToAction("Vacaciones");
    }

    // --- Reemplazos por ausencia (asignados directamente por Admin) ---

    public async Task<IActionResult> Reemplazos()
    {
        return View(await _turnos.ObtenerReemplazosAsync());
    }

    public async Task<IActionResult> CrearReemplazo()
    {
        ViewBag.Empleados = await _db.Empleados.Where(e => e.Activo).OrderBy(e => e.Rol).ThenBy(e => e.Nombre).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearReemplazo(RolEmpleado rol, int empleadoDestinoId, DateTime fechaInicio, DateTime fechaFin, string motivo, string? ausente)
    {
        try
        {
            await _turnos.CrearReemplazoAsync(rol, empleadoDestinoId, fechaInicio, fechaFin, motivo, ausente, EmpleadoIdActual);
            TempData["Mensaje"] = "Reemplazo asignado y notificado al empleado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Reemplazos");
    }
}
