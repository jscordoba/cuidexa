using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Auxiliar")]
public class AuxiliaresController : Controller
{
    private readonly IEventoDistribucionService _eventos;
    private readonly ITurnoService _turnos;
    private readonly IVacacionService _vacaciones;
    private readonly IGrupoNotificacionService _grupos;
    private readonly IIncidenciaService _incidencias;
    private readonly CuidexaDbContext _db;

    public AuxiliaresController(IEventoDistribucionService eventos, ITurnoService turnos, IVacacionService vacaciones, IGrupoNotificacionService grupos, IIncidenciaService incidencias, CuidexaDbContext db)
    {
        _eventos = eventos;
        _turnos = turnos;
        _vacaciones = vacaciones;
        _grupos = grupos;
        _incidencias = incidencias;
        _db = db;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    public async Task<IActionResult> Index()
    {
        var avisos = await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Auxiliar, EmpleadoIdActual ?? 0);
        var turnosEspeciales = await _turnos.ObtenerTurnosEspecialesAsync(EmpleadoIdActual ?? 0);

        ViewBag.AvisosPendientes = avisos.Count(e => !e.Leido);
        ViewBag.TurnosEspeciales = turnosEspeciales.Count;
        return View();
    }

    public async Task<IActionResult> Avisos()
    {
        return View(await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Auxiliar, EmpleadoIdActual ?? 0));
    }

    [HttpPost]
    public async Task<IActionResult> MarcarLeido(int id)
    {
        await _eventos.MarcarLeidoAsync(id);
        return RedirectToAction("Avisos");
    }

    public async Task<IActionResult> CrearAviso()
    {
        return View("~/Views/Shared/CrearAviso.cshtml", new CrearAvisoViewModel
        {
            Residentes = await _db.Residentes.Where(r => r.Estado == Models.Enums.EstadoResidente.Activo)
                .Include(r => r.Habitacion).OrderBy(r => r.Nombre).ToListAsync(),
            RolOrigen = RolEmpleado.Auxiliar,
            RutaBase = "Auxiliares",
            GruposDisponibles = await _grupos.ObtenerActivosAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearAviso(int? residenteId, string descripcion, List<RolEmpleado>? rolesAdicionales,
        bool urgente, bool todoElCentro, int? grupoDestinoId)
    {
        await _eventos.CrearAvisoManualAsync(residenteId, descripcion, RolEmpleado.Auxiliar, rolesAdicionales ?? new(), EmpleadoIdActual,
            urgente, todoElCentro, grupoDestinoId);
        TempData["Mensaje"] = "Aviso enviado.";
        return RedirectToAction("Avisos");
    }

    public async Task<IActionResult> Turnos()
    {
        var empleadoId = EmpleadoIdActual ?? 0;
        var empleado = await _db.Empleados.Include(e => e.PlantillaTurnoDefecto).FirstOrDefaultAsync(e => e.Id == empleadoId);
        return View("~/Views/Shared/MisTurnos.cshtml", new MisTurnosViewModel
        {
            PlantillaDefecto = empleado?.PlantillaTurnoDefecto,
            EmpleadoIdPropio = empleadoId,
            TurnosEspeciales = await _turnos.ObtenerTurnosEspecialesAsync(empleadoId),
            Companeros = await _turnos.ObtenerCompanerosAsync(RolEmpleado.Auxiliar, empleadoId),
            MisCambios = await _turnos.ObtenerCambiosDeAsync(empleadoId),
            MisVacaciones = await _vacaciones.ObtenerPorEmpleadoAsync(empleadoId),
            RolPropio = RolEmpleado.Auxiliar,
            RutaBase = "Auxiliares"
        });
    }

    [HttpPost]
    public async Task<IActionResult> SolicitarCambio(DateOnly fecha, string motivo, int? empleadoSustitutoPropuestoId)
    {
        try
        {
            await _turnos.SolicitarCambioAsync(fecha, EmpleadoIdActual ?? 0, motivo, empleadoSustitutoPropuestoId);
            TempData["Mensaje"] = "Cambio de turno solicitado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Turnos");
    }

    [HttpPost]
    public async Task<IActionResult> SolicitarVacaciones(DateOnly fechaInicio, DateOnly fechaFin, string? notas)
    {
        try
        {
            await _vacaciones.SolicitarAsync(EmpleadoIdActual ?? 0, fechaInicio, fechaFin, notas);
            TempData["Mensaje"] = "Vacaciones solicitadas.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Turnos");
    }

    public async Task<IActionResult> Incidencias()
    {
        return View("~/Views/Shared/Incidencias.cshtml", new IncidenciasViewModel
        {
            Incidencias = await _incidencias.ObtenerParaRolAsync(RolEmpleado.Auxiliar),
            RutaBase = "Auxiliares"
        });
    }

    public async Task<IActionResult> CrearIncidencia()
    {
        return View("~/Views/Shared/CrearIncidencia.cshtml", new CrearIncidenciaViewModel
        {
            Residentes = await _db.Residentes.Where(r => r.Estado == Models.Enums.EstadoResidente.Activo)
                .Include(r => r.Habitacion).OrderBy(r => r.Nombre).ToListAsync(),
            RolOrigen = RolEmpleado.Auxiliar,
            RutaBase = "Auxiliares"
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearIncidencia(Models.Enums.TipoIncidencia tipo, int? residenteId, string titulo, string descripcion, Models.Enums.GravedadIncidencia gravedad)
    {
        try
        {
            await _incidencias.CrearAsync(tipo, residenteId, titulo, descripcion, gravedad, RolEmpleado.Auxiliar, EmpleadoIdActual);
            TempData["Mensaje"] = "Incidencia reportada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Incidencias");
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
