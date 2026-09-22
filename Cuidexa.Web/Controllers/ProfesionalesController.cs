using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Profesional")]
public class ProfesionalesController : Controller
{
    private readonly IProfesionalesService _profesionales;
    private readonly IEventoDistribucionService _eventos;
    private readonly ITurnoService _turnos;
    private readonly IVacacionService _vacaciones;
    private readonly IGrupoNotificacionService _grupos;
    private readonly CuidexaDbContext _db;

    public ProfesionalesController(IProfesionalesService profesionales, IEventoDistribucionService eventos, ITurnoService turnos, IVacacionService vacaciones, IGrupoNotificacionService grupos, CuidexaDbContext db)
    {
        _profesionales = profesionales;
        _eventos = eventos;
        _turnos = turnos;
        _vacaciones = vacaciones;
        _grupos = grupos;
        _db = db;
    }

    // Nunca null en la práctica: solo se llega aquí autenticado con el claim
    // EmpleadoId (ver AccountController), pero si faltara preferimos que
    // reviente aquí a que un profesional actúe como si tuviera Id 0.
    private int EmpleadoIdActual => int.Parse(User.FindFirst("EmpleadoId")!.Value);

    // Dashboard: solo cifras — el detalle de cada cosa vive en su propia
    // pantalla (Avisos/Agenda/Residentes/Turnos).
    public async Task<IActionResult> Index()
    {
        var avisos = await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Profesional, EmpleadoIdActual);
        var agenda = await _profesionales.ObtenerAgendaAsync(EmpleadoIdActual);
        var residentes = await _profesionales.ObtenerResidentesAsync();
        var turnosEspeciales = await _turnos.ObtenerTurnosEspecialesAsync(EmpleadoIdActual);

        ViewBag.AvisosPendientes = avisos.Count(e => !e.Leido);
        ViewBag.SesionesProgramadas = agenda.Count;
        ViewBag.ResidentesActivos = residentes.Count;
        ViewBag.TurnosEspeciales = turnosEspeciales.Count;
        return View();
    }

    public async Task<IActionResult> Turnos()
    {
        var empleado = await _db.Empleados.Include(e => e.PlantillaTurnoDefecto).FirstOrDefaultAsync(e => e.Id == EmpleadoIdActual);
        return View("~/Views/Shared/MisTurnos.cshtml", new MisTurnosViewModel
        {
            PlantillaDefecto = empleado?.PlantillaTurnoDefecto,
            EmpleadoIdPropio = EmpleadoIdActual,
            TurnosEspeciales = await _turnos.ObtenerTurnosEspecialesAsync(EmpleadoIdActual),
            Companeros = await _turnos.ObtenerCompanerosAsync(RolEmpleado.Profesional, EmpleadoIdActual),
            MisCambios = await _turnos.ObtenerCambiosDeAsync(EmpleadoIdActual),
            MisVacaciones = await _vacaciones.ObtenerPorEmpleadoAsync(EmpleadoIdActual),
            RolPropio = RolEmpleado.Profesional,
            RutaBase = "Profesionales"
        });
    }

    [HttpPost]
    public async Task<IActionResult> SolicitarCambio(DateOnly fecha, string motivo, int? empleadoSustitutoPropuestoId)
    {
        try
        {
            await _turnos.SolicitarCambioAsync(fecha, EmpleadoIdActual, motivo, empleadoSustitutoPropuestoId);
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
            await _vacaciones.SolicitarAsync(EmpleadoIdActual, fechaInicio, fechaFin, notas);
            TempData["Mensaje"] = "Vacaciones solicitadas.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Turnos");
    }

    public async Task<IActionResult> Avisos()
    {
        return View(await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Profesional, EmpleadoIdActual));
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
            RolOrigen = RolEmpleado.Profesional,
            RutaBase = "Profesionales",
            GruposDisponibles = await _grupos.ObtenerActivosAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearAviso(int? residenteId, string descripcion, List<RolEmpleado>? rolesAdicionales,
        bool urgente, bool todoElCentro, int? grupoDestinoId)
    {
        await _eventos.CrearAvisoManualAsync(residenteId, descripcion, RolEmpleado.Profesional, rolesAdicionales ?? new(), EmpleadoIdActual,
            urgente, todoElCentro, grupoDestinoId);
        TempData["Mensaje"] = "Aviso enviado.";
        return RedirectToAction("Avisos");
    }

    public async Task<IActionResult> Agenda()
    {
        return View(await _profesionales.ObtenerAgendaAsync(EmpleadoIdActual));
    }

    public async Task<IActionResult> Residentes()
    {
        return View(await _profesionales.ObtenerResidentesAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var residente = await _profesionales.ObtenerResidenteAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Sesiones = await _profesionales.ObtenerSesionesAsync(id, EmpleadoIdActual);
        ViewBag.Historial = await _profesionales.ObtenerHistorialAsync(id, EmpleadoIdActual);
        return View(residente);
    }

    [HttpPost]
    public async Task<IActionResult> ProgramarSesion(int residenteId, DateTime fechaHora)
    {
        await _profesionales.ProgramarSesionAsync(residenteId, EmpleadoIdActual, fechaHora);
        return RedirectToAction("Details", new { id = residenteId });
    }

    [HttpPost]
    public async Task<IActionResult> MarcarRealizada(int residenteId, int sesionId, string? observaciones)
    {
        await _profesionales.MarcarRealizadaAsync(sesionId, EmpleadoIdActual, observaciones);
        return RedirectToAction("Details", new { id = residenteId });
    }

    [HttpPost]
    public async Task<IActionResult> CancelarSesion(int residenteId, int sesionId)
    {
        await _profesionales.CancelarSesionAsync(sesionId, EmpleadoIdActual);
        return RedirectToAction("Details", new { id = residenteId });
    }
}
