using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using WebPush;

namespace Cuidexa.Web.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly CuidexaDbContext _db;
    private readonly VapidDetails _vapidDetails;
    private readonly WebPushClient _client = new();
    private readonly ITenantContext _tenant;

    public PushNotificationService(CuidexaDbContext db, IConfiguration configuration, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
        _vapidDetails = new VapidDetails(
            configuration["WebPush:Subject"],
            configuration["WebPush:PublicKey"],
            configuration["WebPush:PrivateKey"]);
    }

    public async Task SuscribirAsync(int empleadoId, string endpoint, string p256dh, string auth)
    {
        var existente = await _db.SuscripcionesPush.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            existente.EmpleadoId = empleadoId;
            existente.P256dh = p256dh;
            existente.Auth = auth;
        }
        else
        {
            _db.SuscripcionesPush.Add(new SuscripcionPush
            {
                CentroId = _tenant.CentroId,
                EmpleadoId = empleadoId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task DesuscribirAsync(string endpoint)
    {
        var existente = await _db.SuscripcionesPush.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            _db.SuscripcionesPush.Remove(existente);
            await _db.SaveChangesAsync();
        }
    }

    // Ruta de la bandeja de avisos de cada rol — Dirección no tiene bandeja
    // propia (ver CrearAvisoViewModel.DepartamentosDisponibles), así que un
    // grupo que incluyera a alguien de Dirección cae al inicio en vez de
    // romper: caso límite que nadie ha pedido cubrir mejor.
    private static string RutaAvisos(RolEmpleado rol) => rol switch
    {
        RolEmpleado.Admin => "/Admin/Avisos",
        RolEmpleado.Cocina => "/Cocina/Avisos",
        RolEmpleado.Auxiliar => "/Auxiliares/Avisos",
        RolEmpleado.Enfermeria => "/Enfermeria/Avisos",
        RolEmpleado.Profesional => "/Profesionales/Avisos",
        RolEmpleado.Limpieza => "/Limpieza/Avisos",
        _ => "/"
    };

    private static string RutaIncidencias(RolEmpleado rol) => rol switch
    {
        RolEmpleado.Admin => "/Admin/Incidencias",
        RolEmpleado.Cocina => "/Cocina/Incidencias",
        RolEmpleado.Auxiliar => "/Auxiliares/Incidencias",
        RolEmpleado.Enfermeria => "/Enfermeria/Incidencias",
        RolEmpleado.Profesional => "/Profesionales/Incidencias",
        RolEmpleado.Limpieza => "/Limpieza/Incidencias",
        _ => "/"
    };

    public async Task NotificarAsync(IEnumerable<EventoDistribucion> eventosNuevos)
    {
        foreach (var evento in eventosNuevos)
        {
            List<Empleado> destinatarios;
            string url;

            if (evento.EmpleadoDestinoId.HasValue)
            {
                var empleado = await _db.Empleados.FindAsync(evento.EmpleadoDestinoId.Value);
                if (empleado is null || !empleado.Activo) continue;
                destinatarios = new List<Empleado> { empleado };
                url = RutaAvisos(empleado.Rol);
            }
            else if (evento.RolDestino.HasValue)
            {
                destinatarios = await _db.Empleados.Where(e => e.Activo && e.Rol == evento.RolDestino.Value).ToListAsync();
                url = RutaAvisos(evento.RolDestino.Value);
            }
            else
            {
                continue;
            }

            var payload = JsonSerializer.Serialize(new
            {
                titulo = "Cuidexa",
                cuerpo = evento.Descripcion,
                urgente = evento.Urgente,
                url
            });

            await EnviarATodosAsync(destinatarios, payload);
        }
    }

    public async Task NotificarIncidenciaAsync(Incidencia incidencia, RolEmpleado rolDestino)
    {
        var destinatarios = await _db.Empleados.Where(e => e.Activo && e.Rol == rolDestino).ToListAsync();
        var payload = JsonSerializer.Serialize(new
        {
            titulo = "Cuidexa · Incidencia urgente",
            cuerpo = incidencia.Titulo,
            urgente = true,
            url = RutaIncidencias(rolDestino)
        });

        await EnviarATodosAsync(destinatarios, payload);
    }

    private async Task EnviarATodosAsync(List<Empleado> destinatarios, string payload)
    {
        foreach (var destinatario in destinatarios)
        {
            var suscripciones = await _db.SuscripcionesPush.Where(s => s.EmpleadoId == destinatario.Id).ToListAsync();
            foreach (var suscripcion in suscripciones)
            {
                var pushSubscription = new PushSubscription(suscripcion.Endpoint, suscripcion.P256dh, suscripcion.Auth);
                try
                {
                    await _client.SendNotificationAsync(pushSubscription, payload, _vapidDetails);
                }
                catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    // Suscripción caducada/inválida — se limpia sola en vez
                    // de seguir reintentando contra un endpoint muerto.
                    _db.SuscripcionesPush.Remove(suscripcion);
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}
