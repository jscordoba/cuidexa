using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class SoporteService : ISoporteService
{
    private readonly CuidexaDbContext _db;
    private readonly ITenantContext _tenant;

    public SoporteService(CuidexaDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<List<TicketSoporte>> ObtenerParaCentroActualAsync() =>
        await _db.TicketsSoporte
            .Include(t => t.Empleado)
            .OrderByDescending(t => t.FechaCreacion)
            .ToListAsync();

    public async Task CrearTicketAsync(string titulo, string descripcion, int? empleadoId)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
        {
            throw new InvalidOperationException("El título y la descripción son obligatorios.");
        }

        _db.TicketsSoporte.Add(new TicketSoporte
        {
            CentroId = _tenant.CentroId,
            Titulo = titulo.Trim(),
            Descripcion = descripcion.Trim(),
            EmpleadoId = empleadoId
        });
        await _db.SaveChangesAsync();
    }

    // IgnoreQueryFilters: SuperAdmin no tiene CentroId propio (opera por
    // encima del modelo de tenant, igual que con Organizaciones/Centros) y
    // necesita ver los tickets de todos los centros para poder atenderlos.
    public async Task<List<TicketSoporte>> ObtenerTodosAsync() =>
        await _db.TicketsSoporte
            .IgnoreQueryFilters()
            .Include(t => t.Empleado)
            .Include(t => t.Centro).ThenInclude(c => c!.Organizacion)
            .OrderBy(t => t.Estado)
            .ThenByDescending(t => t.FechaCreacion)
            .ToListAsync();

    public async Task ResponderAsync(int ticketId, string respuesta, int superAdminId)
    {
        if (string.IsNullOrWhiteSpace(respuesta))
        {
            throw new InvalidOperationException("La respuesta no puede estar vacía.");
        }

        var ticket = await _db.TicketsSoporte.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == ticketId)
            ?? throw new InvalidOperationException("Ticket no encontrado.");

        ticket.Respuesta = respuesta.Trim();
        ticket.FechaRespuesta = DateTime.UtcNow;
        ticket.RespondidoPorSuperAdminId = superAdminId;
        ticket.Estado = EstadoTicketSoporte.Resuelto;
        await _db.SaveChangesAsync();
    }
}
