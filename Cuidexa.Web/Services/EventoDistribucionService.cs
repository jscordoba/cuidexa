using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class EventoDistribucionService : IEventoDistribucionService
{
    private readonly CuidexaDbContext _db;
    private readonly IGrupoNotificacionService _grupos;
    private readonly IPushNotificationService _push;
    private readonly ITenantContext _tenant;

    // Roles con bandeja de avisos propia — Dirección no tiene (ver
    // CrearAvisoViewModel.DepartamentosDisponibles), así que "todo el
    // centro" no le manda nada, igual que ya pasa con los adicionales.
    private static readonly RolEmpleado[] RolesConBandeja =
        { RolEmpleado.Admin, RolEmpleado.Cocina, RolEmpleado.Auxiliar, RolEmpleado.Enfermeria, RolEmpleado.Profesional, RolEmpleado.Limpieza };

    public EventoDistribucionService(CuidexaDbContext db, IGrupoNotificacionService grupos, IPushNotificationService push, ITenantContext tenant)
    {
        _db = db;
        _grupos = grupos;
        _push = push;
        _tenant = tenant;
    }

    public async Task<List<EventoDistribucion>> ObtenerPorRolAsync(RolEmpleado rol)
    {
        return await _db.EventosDistribucion
            .Include(e => e.Residente)
            .Include(e => e.EmpleadoOrigen)
            .Where(e => e.RolDestino == rol)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<EventoDistribucion>> ObtenerParaEmpleadoAsync(RolEmpleado rol, int empleadoId)
    {
        return await _db.EventosDistribucion
            .Include(e => e.Residente)
            .Include(e => e.EmpleadoOrigen)
            .Where(e => e.RolDestino == rol || e.EmpleadoDestinoId == empleadoId)
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync();
    }

    public async Task MarcarLeidoAsync(int eventoId)
    {
        var evento = await _db.EventosDistribucion.FindAsync(eventoId);
        if (evento is not null)
        {
            evento.Leido = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task CrearAvisoManualAsync(int? residenteId, string descripcion, RolEmpleado rolOrigen,
        IEnumerable<RolEmpleado> rolesAdicionales, int? empleadoOrigenId,
        bool urgente, bool todoElCentro, int? grupoDestinoId)
    {
        if (residenteId.HasValue)
        {
            var existeResidente = await _db.Residentes.AnyAsync(r => r.Id == residenteId.Value);
            if (!existeResidente)
            {
                throw new InvalidOperationException("Residente no encontrado.");
            }
        }

        // El propio departamento y Admin son obligatorios; los adicionales son
        // lo que el emisor haya marcado — salvo que se pida "todo el centro",
        // que sustituye la selección por todos los roles con bandeja.
        HashSet<RolEmpleado> destinos;
        if (todoElCentro)
        {
            destinos = new HashSet<RolEmpleado>(RolesConBandeja);
        }
        else
        {
            destinos = new HashSet<RolEmpleado> { rolOrigen, RolEmpleado.Admin };
            foreach (var rol in rolesAdicionales)
            {
                destinos.Add(rol);
            }
        }

        var creados = new List<EventoDistribucion>();

        foreach (var destino in destinos)
        {
            var evento = new EventoDistribucion
            {
                CentroId = _tenant.CentroId,
                ResidenteId = residenteId,
                TipoEvento = TipoEvento.AvisoManual,
                RolDestino = destino,
                RolOrigen = rolOrigen,
                EmpleadoOrigenId = empleadoOrigenId,
                Urgente = urgente,
                Descripcion = descripcion
            };
            _db.EventosDistribucion.Add(evento);
            creados.Add(evento);
        }

        if (grupoDestinoId.HasValue)
        {
            var miembros = await _grupos.ObtenerMiembrosAsync(grupoDestinoId.Value);
            foreach (var miembro in miembros)
            {
                // Si el departamento del miembro ya está entre los destinos
                // (el suyo propio, uno adicional marcado, o "todo el centro"),
                // ya le va a llegar por ahí — una fila personal duplicaría el
                // aviso en su bandeja en vez de ser un destino adicional.
                if (destinos.Contains(miembro.Rol)) continue;

                var evento = new EventoDistribucion
                {
                    CentroId = _tenant.CentroId,
                    ResidenteId = residenteId,
                    TipoEvento = TipoEvento.AvisoManual,
                    EmpleadoDestinoId = miembro.Id,
                    RolOrigen = rolOrigen,
                    EmpleadoOrigenId = empleadoOrigenId,
                    Urgente = urgente,
                    Descripcion = descripcion
                };
                _db.EventosDistribucion.Add(evento);
                creados.Add(evento);
            }
        }

        await _db.SaveChangesAsync();
        await _push.NotificarAsync(creados);
    }
}
