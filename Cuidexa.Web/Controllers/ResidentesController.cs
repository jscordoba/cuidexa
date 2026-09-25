using System.Security.Claims;
using Cuidexa.Web.Data;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

[Authorize(Roles = "Admin,DirectorOrganizacion")]
public class ResidentesController : Controller
{
    private readonly IResidenteService _residentes;
    private readonly IAuditService _auditoria;
    private readonly IOrganizacionService _organizacion;
    private readonly ICentroService _centro;
    private readonly ITenantContext _tenant;
    private readonly IDocumentoFirmadoService _documentos;
    private readonly IFamiliarService _familiares;
    private readonly CuidexaDbContext _db;

    public ResidentesController(IResidenteService residentes, IAuditService auditoria, IOrganizacionService organizacion,
        ICentroService centro, ITenantContext tenant, IDocumentoFirmadoService documentos, IFamiliarService familiares, CuidexaDbContext db)
    {
        _residentes = residentes;
        _auditoria = auditoria;
        _organizacion = organizacion;
        _centro = centro;
        _tenant = tenant;
        _documentos = documentos;
        _familiares = familiares;
        _db = db;
    }

    private int? EmpleadoIdActual =>
        int.TryParse(User.FindFirst("EmpleadoId")?.Value, out var id) ? id : null;

    // Habitaciones etiquetadas con su centro solo cuando hace falta
    // distinguir entre varios (DirectorOrganizacion) — para Admin (un único
    // centro posible) es ruido innecesario.
    private async Task<List<(int Id, string Etiqueta)>> ObtenerHabitacionesParaFormularioAsync()
    {
        var habitaciones = await _db.Habitaciones.Include(h => h.Centro).OrderBy(h => h.Numero).ToListAsync();
        return habitaciones
            .Select(h => (h.Id, _tenant.AccesoOrganizacionCompleto ? $"{h.Numero} ({h.Planta}) — {h.Centro?.Nombre}" : $"{h.Numero} ({h.Planta})"))
            .ToList();
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _residentes.ObtenerTodosAsync();
        ViewBag.MostrarCentro = _tenant.AccesoOrganizacionCompleto;
        return View(lista);
    }

    public async Task<IActionResult> Details(int id)
    {
        var residente = await _residentes.ObtenerFichaCompletaAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Historial = await _auditoria.ObtenerPorEntidadAsync("Residente", id);
        // Solo habitaciones del propio centro del residente — un traslado
        // nunca cruza de centro (ver ResidenteService.TrasladarAsync), así
        // que ofrecer las de otro centro de la organización solo confundiría.
        ViewBag.Habitaciones = await _db.Habitaciones.Where(h => h.CentroId == residente.CentroId).ToListAsync();
        return View(residente);
    }

    public async Task<IActionResult> FichaPdf(int id)
    {
        var residente = await _residentes.ObtenerFichaCompletaAsync(id);
        if (residente is null) return NotFound();

        var pdf = FichaPdfGenerator.Generar(residente, await _organizacion.ObtenerAsync());
        var nombreArchivo = $"ficha-{residente.Nombre}-{residente.Apellidos}.pdf".Replace(" ", "-").ToLowerInvariant();
        return File(pdf, "application/pdf", nombreArchivo);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Habitaciones = await ObtenerHabitacionesParaFormularioAsync();
        ViewBag.Dietas = await _db.Dietas.ToListAsync();
        ViewBag.Alergias = await _db.Alergias.ToListAsync();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(new ResidenteCreateDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ResidenteCreateDto dto, int? centroId)
    {
        var esValido = dto.EsValido(out var error);
        if (!esValido)
        {
            ModelState.AddModelError("", error);
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _residentes.DarDeAltaAsync(dto, EmpleadoIdActual, centroId);
                TempData["Mensaje"] = "Residente dado de alta. Cocina y Auxiliares ya tienen la información en su panel.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        ViewBag.Habitaciones = await ObtenerHabitacionesParaFormularioAsync();
        ViewBag.Dietas = await _db.Dietas.ToListAsync();
        ViewBag.Alergias = await _db.Alergias.ToListAsync();
        ViewBag.CentrosOrganizacion = _tenant.AccesoOrganizacionCompleto ? await _centro.ObtenerCentrosDeMiOrganizacionAsync() : null;
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var residente = await _residentes.ObtenerPorIdAsync(id);
        if (residente is null) return NotFound();

        var dietaVigente = residente.Dietas.FirstOrDefault(d => d.FechaFin == null);
        var dto = new ResidenteEditDto
        {
            Nombre = residente.Nombre,
            Apellidos = residente.Apellidos,
            DocumentoIdentidad = residente.DocumentoIdentidad,
            FechaNacimiento = residente.FechaNacimiento,
            TipoResidente = residente.TipoResidente,
            Movilidad = residente.Movilidad,
            NecesitaAyudaLevantarse = residente.NecesitaAyudaLevantarse,
            EquipamientoEspecial = residente.EquipamientoEspecial,
            Telefono = residente.Telefono,
            Email = residente.Email,
            ContactoEmergenciaNombre = residente.ContactoEmergenciaNombre,
            ContactoEmergenciaRelacion = residente.ContactoEmergenciaRelacion,
            ContactoEmergenciaTelefono = residente.ContactoEmergenciaTelefono,
            ContactoEmergenciaEmail = residente.ContactoEmergenciaEmail,
            DietaId = dietaVigente?.DietaId ?? 0,
            AlergiaIds = residente.Alergias.Select(a => a.AlergiaId).ToList()
        };

        ViewBag.ResidenteId = id;
        ViewBag.Nombre = $"{residente.Nombre} {residente.Apellidos}";
        ViewBag.Dietas = await _db.Dietas.ToListAsync();
        ViewBag.Alergias = await _db.Alergias.ToListAsync();
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ResidenteEditDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResidenteId = id;
            ViewBag.Dietas = await _db.Dietas.ToListAsync();
            ViewBag.Alergias = await _db.Alergias.ToListAsync();
            return View(dto);
        }

        await _residentes.ActualizarAsync(id, dto, EmpleadoIdActual);
        TempData["Mensaje"] = "Datos del residente actualizados.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Baja(int id, string? motivo)
    {
        await _residentes.DarDeBajaAsync(id, EmpleadoIdActual, motivo);
        TempData["Mensaje"] = "Residente dado de baja. Cocina y Auxiliares han sido informados.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Trasladar(int id, int nuevaHabitacionId)
    {
        await _residentes.TrasladarAsync(id, nuevaHabitacionId, EmpleadoIdActual);
        TempData["Mensaje"] = "Residente trasladado. Auxiliares han sido informados.";
        return RedirectToAction("Details", new { id });
    }

    // Documentos firmados (consentimientos, admisión, entregas...) del
    // residente — firma capturada en pantalla, no firma digital con validez
    // legal (ver DocumentoFirmado). Sección propia en vez de mezclarse en
    // Details/Edit: es un histórico que crece, no un dato del residente.
    public async Task<IActionResult> Documentos(int id)
    {
        var residente = await _residentes.ObtenerPorIdAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Residente = residente;
        return View(await _documentos.ObtenerPorResidenteAsync(id));
    }

    public async Task<IActionResult> CrearDocumento(int id)
    {
        var residente = await _residentes.ObtenerPorIdAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Residente = residente;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearDocumento(int id, Models.Enums.CategoriaDocumentoFirmado categoria, string titulo,
        string descripcion, string firmanteNombre, string firmanteRelacion, string firmaImagenBase64)
    {
        try
        {
            await _documentos.CrearAsync(categoria, titulo, descripcion, id, null, firmanteNombre, firmanteRelacion,
                firmaImagenBase64, EmpleadoIdActual);
            TempData["Mensaje"] = "Documento firmado guardado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Documentos", new { id });
    }

    // Portal de familiares (Fase 10 — comercialización, bloque 5): Admin
    // invita al familiar desde la ficha del residente (nombre, relación,
    // email, contraseña inicial) — mismo patrón manual ya usado para
    // Empleados/SuperAdmin, sin depender de email transaccional.
    public async Task<IActionResult> Familiares(int id)
    {
        var residente = await _residentes.ObtenerPorIdAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Residente = residente;
        return View(await _familiares.ObtenerPorResidenteAsync(id));
    }

    public async Task<IActionResult> CrearFamiliar(int id)
    {
        var residente = await _residentes.ObtenerPorIdAsync(id);
        if (residente is null) return NotFound();

        ViewBag.Residente = residente;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearFamiliar(int id, string nombre, string relacion, string email, string password)
    {
        try
        {
            await _familiares.CrearAsync(id, nombre, relacion, email, password, EmpleadoIdActual);
            TempData["Mensaje"] = $"Acceso del portal creado para {nombre}. Comparte con la familia el email y la contraseña iniciales.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Familiares", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstadoFamiliar(int id, int familiarId, bool activo)
    {
        try
        {
            await _familiares.CambiarEstadoAsync(id, familiarId, activo, EmpleadoIdActual);
            TempData["Mensaje"] = activo ? "Acceso reactivado." : "Acceso desactivado — ya no puede iniciar sesión.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Familiares", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> RestablecerPasswordFamiliar(int id, int familiarId, string nuevaPassword)
    {
        try
        {
            await _familiares.RestablecerPasswordAsync(id, familiarId, nuevaPassword, EmpleadoIdActual);
            TempData["Mensaje"] = "Contraseña restablecida. Comparte la nueva contraseña con la familia por un canal aparte.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Mensaje"] = ex.Message;
        }
        return RedirectToAction("Familiares", new { id });
    }
}
