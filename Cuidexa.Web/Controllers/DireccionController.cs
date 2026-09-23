using Cuidexa.Web.Data;
using Cuidexa.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Controllers;

// Dirección es de solo lectura en este MVP: no gestiona, solo visualiza.
[Authorize(Roles = "Direccion")]
public class DireccionController : Controller
{
    private readonly CuidexaDbContext _db;

    public DireccionController(CuidexaDbContext db)
    {
        _db = db;
    }

    // Roles operativos que reciben avisos (se excluyen Admin/Dirección, que no
    // son destino de EventoDistribucion). Generado a partir del enum en vez de
    // listar cada rol a mano: un rol nuevo aparece aquí solo con añadirlo a
    // RolEmpleado, sin tocar este controlador.
    private static readonly RolEmpleado[] RolesOperativos = Enum.GetValues<RolEmpleado>()
        .Where(r => r != RolEmpleado.Admin && r != RolEmpleado.Direccion)
        .ToArray();

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalResidentes = await _db.Residentes.CountAsync(r => r.Estado == EstadoResidente.Activo);

        var pendientesPorRol = new Dictionary<RolEmpleado, int>();
        foreach (var rol in RolesOperativos)
        {
            pendientesPorRol[rol] = await _db.EventosDistribucion.CountAsync(e => e.RolDestino == rol && !e.Leido);
        }
        ViewBag.PendientesPorRol = pendientesPorRol;

        ViewBag.TareasLimpiezaPendientes = await _db.TareasLimpieza.CountAsync(t => t.Estado == EstadoTarea.Pendiente);
        ViewBag.IncidenciasAbiertas = await _db.Incidencias.CountAsync(i => i.Estado != EstadoIncidencia.Resuelta);

        var residentes = await _db.Residentes.Include(r => r.Habitacion).OrderBy(r => r.Nombre).ToListAsync();
        return View(residentes);
    }
}
