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

    public async Task SuscribirSuperAdminAsync(int superAdminId, string endpoint, string p256dh, string auth)
    {
        var existente = await _db.SuscripcionesPushSuperAdmin.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            existente.SuperAdminId = superAdminId;
            existente.P256dh = p256dh;
            existente.Auth = auth;
        }
        else
        {
            _db.SuscripcionesPushSuperAdmin.Add(new SuscripcionPushSuperAdmin
            {
                SuperAdminId = superAdminId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task DesuscribirSuperAdminAsync(string endpoint)
    {
        var existente = await _db.SuscripcionesPushSuperAdmin.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            _db.SuscripcionesPushSuperAdmin.Remove(existente);
            await _db.SaveChangesAsync();
        }
    }

    public async Task SuscribirFamiliarAsync(int familiarId, string endpoint, string p256dh, string auth)
    {
        var existente = await _db.SuscripcionesPushFamiliar.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            existente.FamiliarId = familiarId;
            existente.P256dh = p256dh;
            existente.Auth = auth;
        }
        else
        {
            _db.SuscripcionesPushFamiliar.Add(new SuscripcionPushFamiliar
            {
                FamiliarId = familiarId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task DesuscribirFamiliarAsync(string endpoint)
    {
        var existente = await _db.SuscripcionesPushFamiliar.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existente is not null)
        {
            _db.SuscripcionesPushFamiliar.Remove(existente);
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
                await EnviarUnoAsync(suscripcion.Endpoint, suscripcion.P256dh, suscripcion.Auth, payload,
                    () => _db.SuscripcionesPush.Remove(suscripcion));
            }
        }
    }

    public async Task NotificarNuevoTicketSoporteAsync(TicketSoporte ticket)
    {
        var superAdmins = await _db.SuperAdmins.Where(s => s.Activo).ToListAsync();
        var payload = JsonSerializer.Serialize(new
        {
            titulo = "Cuidexa · Nuevo ticket de soporte",
            cuerpo = ticket.Titulo,
            urgente = false,
            url = "/SuperAdmin/Soporte"
        });

        foreach (var superAdmin in superAdmins)
        {
            var suscripciones = await _db.SuscripcionesPushSuperAdmin.Where(s => s.SuperAdminId == superAdmin.Id).ToListAsync();
            foreach (var suscripcion in suscripciones)
            {
                await EnviarUnoAsync(suscripcion.Endpoint, suscripcion.P256dh, suscripcion.Auth, payload,
                    () => _db.SuscripcionesPushSuperAdmin.Remove(suscripcion));
            }
        }
    }

    public async Task NotificarFamiliarAsync(int residenteId, string titulo, string cuerpo)
    {
        var familiares = await _db.Familiares.Where(f => f.ResidenteId == residenteId && f.Activo).ToListAsync();
        var payload = JsonSerializer.Serialize(new { titulo, cuerpo, urgente = false, url = "/Familiar/Index" });

        foreach (var familiar in familiares)
        {
            var suscripciones = await _db.SuscripcionesPushFamiliar.Where(s => s.FamiliarId == familiar.Id).ToListAsync();
            foreach (var suscripcion in suscripciones)
            {
                await EnviarUnoAsync(suscripcion.Endpoint, suscripcion.P256dh, suscripcion.Auth, payload,
                    () => _db.SuscripcionesPushFamiliar.Remove(suscripcion));
            }
        }
    }

    private async Task EnviarUnoAsync(string endpoint, string p256dh, string auth, string payload, Action eliminarSuscripcion)
    {
        var pushSubscription = new PushSubscription(endpoint, p256dh, auth);
        try
        {
            await _client.SendNotificationAsync(pushSubscription, payload, _vapidDetails);
        }
        catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
        {
            // Suscripción caducada/inválida — se limpia sola en vez de
            // seguir reintentando contra un endpoint muerto.
            eliminarSuscripcion();
            await _db.SaveChangesAsync();
        }
    }
}
