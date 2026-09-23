using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class IncidenciaService : IIncidenciaService
{
    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly IPushNotificationService _push;
    private readonly ITenantContext _tenant;

    public IncidenciaService(CuidexaDbContext db, IAuditService auditoria, IPushNotificationService push, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _push = push;
        _tenant = tenant;
    }

    // Enfermería resuelve las de residente (lo más ligado a cuidado clínico);
    // Admin resuelve personal/operativas e instalaciones/seguridad — no hay
    // un rol operativo obvio dueño de esas dos, y Admin ya es quien gestiona
    // personal y el propio centro en el resto de la app.
    public RolEmpleado RolResponsable(TipoIncidencia tipo) => tipo switch
    {
        TipoIncidencia.Residente => RolEmpleado.Enfermeria,
        TipoIncidencia.Personal => RolEmpleado.Admin,
        TipoIncidencia.Instalaciones => RolEmpleado.Admin,
        _ => RolEmpleado.Admin
    };

    public async Task<List<Incidencia>> ObtenerParaRolAsync(RolEmpleado rol)
    {
        // RolesQueResuelven no es traducible a SQL (es un iterador en C#) —
        // se trae todo lo del centro (ya acotado por el filtro global) y se
        // filtra en memoria. Volumen bajo por centro, mismo patrón que ya
        // usa InformesService para sus agrupaciones.
        var todas = await _db.Incidencias
            .Include(i => i.Residente)
            .Include(i => i.EmpleadoOrigen)
            .Include(i => i.ResueltoPor)
            .Include(i => i.DocumentosFirmados)
            .OrderByDescending(i => i.FechaCreacion)
            .ToListAsync();

        return todas.Where(i => i.RolOrigen == rol || RolesQueResuelven(i.Tipo).Contains(rol)).ToList();
    }

    public async Task CrearAsync(TipoIncidencia tipo, int? residenteId, string titulo, string descripcion,
        GravedadIncidencia gravedad, CaracterIncidencia caracter, RolEmpleado rolOrigen, int? empleadoOrigenId)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descripcion))
        {
            throw new InvalidOperationException("Título y descripción son obligatorios.");
        }

        if (tipo == TipoIncidencia.Residente)
        {
            if (!residenteId.HasValue)
            {
                throw new InvalidOperationException("Elige el residente al que afecta esta incidencia.");
            }
            var existeResidente = await _db.Residentes.AnyAsync(r => r.Id == residenteId.Value);
            if (!existeResidente)
            {
                throw new InvalidOperationException("Residente no encontrado.");
            }
        }
        else
        {
            residenteId = null;
        }

        // Informativa: se da por cerrada al crearla — nunca ocupa la bandeja
        // de "pendientes" de nadie ni el contador de Dirección, aunque queda
        // igual en el historial del centro.
        var esInformativa = caracter == CaracterIncidencia.Informativa;

        var incidencia = new Incidencia
        {
            CentroId = _tenant.CentroId,
            Tipo = tipo,
            ResidenteId = residenteId,
            Titulo = titulo,
            Descripcion = descripcion,
            Gravedad = gravedad,
            Caracter = caracter,
            Estado = esInformativa ? EstadoIncidencia.Resuelta : EstadoIncidencia.Abierta,
            RolOrigen = rolOrigen,
            EmpleadoOrigenId = empleadoOrigenId
        };
        _db.Incidencias.Add(incidencia);
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoOrigenId, "Alta", "Incidencia", incidencia.Id,
            $"Incidencia de {tipo} reportada: {titulo}" + (esInformativa ? " (informativa)" : ""));

        // Informativa nunca notifica — por definición no requiere que nadie
        // actúe, así que no tiene sentido interrumpir a nadie con un push.
        if (!esInformativa && gravedad == GravedadIncidencia.Urgente)
        {
            await _push.NotificarIncidenciaAsync(incidencia, RolResponsable(tipo));
        }
    }

    public async Task CambiarEstadoAsync(int id, EstadoIncidencia estado, string? notasResolucion, int? empleadoIdActor)
    {
        var incidencia = await _db.Incidencias.FindAsync(id)
            ?? throw new InvalidOperationException("Incidencia no encontrada.");

        incidencia.Estado = estado;
        if (estado == EstadoIncidencia.Resuelta)
        {
            if (string.IsNullOrWhiteSpace(notasResolucion))
            {
                throw new InvalidOperationException("Indica qué se hizo para resolver la incidencia.");
            }
            incidencia.FechaResolucion = DateTime.UtcNow;
            incidencia.NotasResolucion = notasResolucion;
            incidencia.ResueltoPorId = empleadoIdActor;
        }
        await _db.SaveChangesAsync();

        await _auditoria.RegistrarAsync(empleadoIdActor, "Modificacion", "Incidencia", incidencia.Id,
            $"Incidencia → {estado}" + (string.IsNullOrWhiteSpace(notasResolucion) ? "" : $": {notasResolucion}"));
    }

    // Admin siempre puede ver/resolver cualquier incidencia (oversight),
    // además del rol específicamente responsable de este tipo.
    private IEnumerable<RolEmpleado> RolesQueResuelven(TipoIncidencia tipo)
    {
        yield return RolResponsable(tipo);
        yield return RolEmpleado.Admin;
    }
}
