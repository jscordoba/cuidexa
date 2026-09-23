using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Enfermeria")]
public class EnfermeriaController : Controller
{
    private readonly IEnfermeriaService _enfermeria;
    private readonly IEventoDistribucionService _eventos;
    private readonly ITurnoService _turnos;
    private readonly IVacacionService _vacaciones;
    private readonly IGrupoNotificacionService _grupos;
    private readonly IIncidenciaService _incidencias;
    private readonly CuidexaDbContext _db;

    public EnfermeriaController(IEnfermeriaService enfermeria, IEventoDistribucionService eventos, ITurnoService turnos, IVacacionService vacaciones, IGrupoNotificacionService grupos, IIncidenciaService incidencias, CuidexaDbContext db)
    {
        _enfermeria = enfermeria;
        _eventos = eventos;
        _turnos = turnos;
        _vacaciones = vacaciones;
        _grupos = grupos;
        _incidencias = incidencias;
        _db = db;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    // Dashboard: solo cifras — cuántos avisos, cuánta medicación pendiente,
    // cuántos residentes, cuántos turnos. El detalle de cada cosa vive en su
    // propia pantalla (Avisos/Agenda/Residentes/Turnos), no aquí.
    public async Task<IActionResult> Index()
    {
        var avisos = await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Enfermeria, EmpleadoIdActual ?? 0);
        var agenda = await _enfermeria.ObtenerAgendaHoyAsync();
        var residentes = await _enfermeria.ObtenerResidentesAsync();
        var turnosEspeciales = await _turnos.ObtenerTurnosEspecialesAsync(EmpleadoIdActual ?? 0);

        ViewBag.AvisosPendientes = avisos.Count(e => !e.Leido);
        ViewBag.MedicacionPendiente = agenda.Count(a => !a.AdministradaHoy);
        ViewBag.ResidentesActivos = residentes.Count;
        ViewBag.TurnosEspeciales = turnosEspeciales.Count;
        return View();
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
            Companeros = await _turnos.ObtenerCompanerosAsync(RolEmpleado.Enfermeria, empleadoId),
            MisCambios = await _turnos.ObtenerCambiosDeAsync(empleadoId),
            MisVacaciones = await _vacaciones.ObtenerPorEmpleadoAsync(empleadoId),
            RolPropio = RolEmpleado.Enfermeria,
            RutaBase = "Enfermeria"
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

    public async Task<IActionResult> Avisos()
    {
        return View(await _eventos.ObtenerParaEmpleadoAsync(RolEmpleado.Enfermeria, EmpleadoIdActual ?? 0));
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
            RolOrigen = RolEmpleado.Enfermeria,
            RutaBase = "Enfermeria",
            GruposDisponibles = await _grupos.ObtenerActivosAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearAviso(int? residenteId, string descripcion, List<RolEmpleado>? rolesAdicionales,
        bool urgente, bool todoElCentro, int? grupoDestinoId)
    {
        await _eventos.CrearAvisoManualAsync(residenteId, descripcion, RolEmpleado.Enfermeria, rolesAdicionales ?? new(), EmpleadoIdActual,
            urgente, todoElCentro, grupoDestinoId);
        TempData["Mensaje"] = "Aviso enviado.";
        return RedirectToAction("Avisos");
    }

    public async Task<IActionResult> Agenda()
    {
        return View(await _enfermeria.ObtenerAgendaHoyAsync());
    }

    public async Task<IActionResult> Residentes()
    {
        return View(await _enfermeria.ObtenerResidentesAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var residente = await _enfermeria.ObtenerDetalleAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Historial = await _enfermeria.ObtenerHistorialClinicoAsync(id);
        ViewBag.Patologias = await _db.Patologias.ToListAsync();
        return View(residente);
    }

    [HttpPost]
    public async Task<IActionResult> AgregarPatologia(int residenteId, int patologiaId, string? observaciones)
    {
        await _enfermeria.AgregarPatologiaAsync(residenteId, patologiaId, observaciones, EmpleadoIdActual);
        return RedirectToAction("Details", new { id = residenteId });
    }

    [HttpPost]
    public async Task<IActionResult> QuitarPatologia(int residenteId, int patologiaId)
    {
        await _enfermeria.QuitarPatologiaAsync(residenteId, patologiaId, EmpleadoIdActual);
        return RedirectToAction("Details", new { id = residenteId });
    }

    [HttpPost]
    public async Task<IActionResult> AgregarMedicacion(int residenteId, MedicacionCreateDto dto)
    {
        await _enfermeria.AgregarMedicacionAsync(residenteId, dto, EmpleadoIdActual);
        return RedirectToAction("Details", new { id = residenteId });
    }

    [HttpPost]
    public async Task<IActionResult> FinalizarMedicacion(int residenteId, int medicacionId)
    {
        await _enfermeria.FinalizarMedicacionAsync(medicacionId, EmpleadoIdActual);
        return RedirectToAction("Details", new { id = residenteId });
    }

    // Se llama tanto desde la Agenda (lista general de hoy) como desde la
    // ficha de un residente (Details) — "volver" dice a cuál de las dos
    // pantallas regresar tras registrar la dosis.
    [HttpPost]
    public async Task<IActionResult> RegistrarAdministracion(int residenteId, int medicacionId, string? observaciones, string volver = "Details")
    {
        await _enfermeria.RegistrarAdministracionAsync(medicacionId, observaciones, EmpleadoIdActual);
        return volver == "Agenda" ? RedirectToAction("Agenda") : RedirectToAction("Details", new { id = residenteId });
    }

    public async Task<IActionResult> Incidencias()
    {
        return View("~/Views/Shared/Incidencias.cshtml", new IncidenciasViewModel
        {
            Incidencias = await _incidencias.ObtenerParaRolAsync(RolEmpleado.Enfermeria),
            RutaBase = "Enfermeria"
        });
    }

    public async Task<IActionResult> CrearIncidencia()
    {
        return View("~/Views/Shared/CrearIncidencia.cshtml", new CrearIncidenciaViewModel
        {
            Residentes = await _db.Residentes.Where(r => r.Estado == Models.Enums.EstadoResidente.Activo)
                .Include(r => r.Habitacion).OrderBy(r => r.Nombre).ToListAsync(),
            RolOrigen = RolEmpleado.Enfermeria,
            RutaBase = "Enfermeria"
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearIncidencia(Models.Enums.TipoIncidencia tipo, int? residenteId, string titulo, string descripcion, Models.Enums.GravedadIncidencia gravedad, Models.Enums.CaracterIncidencia caracter)
    {
        try
        {
            await _incidencias.CrearAsync(tipo, residenteId, titulo, descripcion, gravedad, caracter, RolEmpleado.Enfermeria, EmpleadoIdActual);
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
