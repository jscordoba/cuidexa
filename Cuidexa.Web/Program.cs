using System.Threading.RateLimiting;
using Cuidexa.Web.Data;
using Cuidexa.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;

// Community: gratuita para organizaciones pequeñas (el caso de este MVP).
// Exigida por la librería antes de generar cualquier documento.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Logging estructurado (config en appsettings.json, sección "Serilog") —
// consola siempre, fichero con rotación diaria como red de seguridad local
// mientras no haya un agregador externo (Sentry/Application Insights)
// conectado; añadir uno más adelante es solo otro sink en la config, sin
// tocar código.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// AutoValidateAntiforgeryTokenAttribute como filtro global: exige el token
// __RequestVerificationToken en TODO POST/PUT/DELETE/PATCH salvo que la
// acción lleve [IgnoreAntiforgeryToken] explícito. Los GET no se ven
// afectados (la propia protección solo aplica a verbos "no seguros").
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}).AddViewLocalization();

// Multi-idioma (Fase 10 — comercialización, bloque 6): español como cultura
// por defecto y "neutra" (las claves de SharedResource SON el texto en
// español — sin SharedResource.resx no hace falta traducir nada para que
// España siga funcionando igual que hoy), inglés como cultura añadida vía
// Resources/SharedResource.en.resx. Todavía cubre solo login/layout/nav —
// ver COMERCIALIZACION.md, bloque 6.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Cabecera propia para el fetch() de wwwroot/js/push.js (el único POST por
// JS de la app) — el resto de formularios usan el campo oculto que emite
// @Html.AntiForgeryToken() en cada <form method="post">.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// Limita intentos de login por IP (fuerza bruta): 5 peticiones/minuto por
// IP a cada uno de los 3 endpoints de login, sin cola (las que se pasan del
// límite reciben 429 al instante, no esperan turno).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddDbContext<CuidexaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IResidenteService, ResidenteService>();
builder.Services.AddScoped<IEventoDistribucionService, EventoDistribucionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IEnfermeriaService, EnfermeriaService>();
builder.Services.AddScoped<IProfesionalesService, ProfesionalesService>();
builder.Services.AddScoped<ILimpiezaService, LimpiezaService>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IComensalesService, ComensalesService>();
builder.Services.AddScoped<IVacacionService, VacacionService>();
builder.Services.AddScoped<IPlantillaTurnoService, PlantillaTurnoService>();
builder.Services.AddScoped<ICentroService, CentroService>();
builder.Services.AddScoped<IOrganizacionService, OrganizacionService>();
builder.Services.AddScoped<ICuentaDispositivoService, CuentaDispositivoService>();
builder.Services.AddScoped<IGrupoNotificacionService, GrupoNotificacionService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<IInformesService, InformesService>();
builder.Services.AddScoped<IIncidenciaService, IncidenciaService>();
builder.Services.AddScoped<IDocumentoFirmadoService, DocumentoFirmadoService>();
builder.Services.AddScoped<IAnalisisService, AnalisisService>();
builder.Services.AddScoped<ISoporteService, SoporteService>();
builder.Services.AddScoped<IFamiliarService, FamiliarService>();
builder.Services.AddScoped<IRolPresentacionService, RolPresentacionService>();
builder.Services.AddHostedService<BackupService>();

builder.Services.AddHttpClient<IFestivosApiService, FestivosApiService>(cliente =>
{
    cliente.Timeout = TimeSpan.FromSeconds(10);
    cliente.DefaultRequestHeaders.UserAgent.ParseAdd("Cuidexa/1.0");
});

// Segundo esquema de cookie, separado a propósito, para las tablets
// compartidas de la Fase 6 (Controllers/TabletController.cs): al no ser el
// esquema por defecto, un [Authorize(Roles = "...")] normal (sin especificar
// AuthenticationSchemes) nunca reconoce una sesión de tablet — aísla por
// completo "solo consulta" de las acciones de escritura de Empleado.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    })
    .AddCookie(Cuidexa.Web.Controllers.TabletController.EsquemaDispositivo, options =>
    {
        options.Cookie.Name = "Cuidexa.Dispositivo";
        options.LoginPath = "/Tablet/Login";
        options.AccessDeniedPath = "/Tablet/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(90);
        options.SlidingExpiration = true;
    })
    // Tercer esquema, separado de Empleado/Dispositivo (Fase 9): SuperAdmin
    // no tiene CentroId/OrganizacionId propios, opera por encima del modelo
    // de tenant (crea Organizaciones/Centros) — igual que Dispositivo, nunca
    // debe satisfacer un [Authorize(Roles = "...")] de los controladores
    // operativos.
    .AddCookie(Cuidexa.Web.Controllers.SuperAdminController.EsquemaSuperAdmin, options =>
    {
        options.Cookie.Name = "Cuidexa.SuperAdmin";
        options.LoginPath = "/SuperAdmin/Login";
        options.AccessDeniedPath = "/SuperAdmin/Login";
    })
    // Cuarto esquema, separado de Empleado/Dispositivo/SuperAdmin (Fase 10
    // — portal de familiares): un Familiar no tiene CentroId/OrganizacionId
    // propios y su alcance (un único Residente) es más estrecho que el de
    // cualquier rol operativo — nunca debe satisfacer un
    // [Authorize(Roles = "...")] de los controladores de personal.
    .AddCookie(Cuidexa.Web.Controllers.FamiliarController.EsquemaFamiliar, options =>
    {
        options.Cookie.Name = "Cuidexa.Familiar";
        options.LoginPath = "/Familiar/Login";
        options.AccessDeniedPath = "/Familiar/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(90);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Comprueba conectividad real con PostgreSQL — usado por monitores externos
// de disponibilidad (uptime) y por el propio despliegue para saber si el
// servicio está listo antes de recibir tráfico.
builder.Services.AddHealthChecks().AddDbContextCheck<CuidexaDbContext>();

var app = builder.Build();

// Aplica migraciones pendientes automáticamente al arrancar (cómodo en desarrollo;
// en producción es preferible aplicar migraciones como paso explícito de despliegue).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CuidexaDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Una línea estructurada por request (método, ruta, código, duración) — la
// señal mínima para saber que el servicio está vivo y detectar errores/
// lentitud sin instrumentar cada acción a mano. CentroId/Usuario se añaden
// aquí (no vía LogContext) porque este middleware envuelve todo el resto
// del pipeline y escribe su línea de resumen DESPUÉS de que ese "using" ya
// se haya cerrado — EnrichDiagnosticContext se evalúa en el momento
// correcto, justo antes de escribir la línea.
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CentroId", httpContext.User.FindFirst("CentroId")?.Value);
        diagnosticContext.Set("Usuario", httpContext.User.Identity?.Name);
    };
});

app.UseHttpsRedirection();

// Cultura por cookie (CookieRequestCultureProvider, incluido en el
// framework) — CulturaController.Cambiar la fija; sin cookie, español por
// defecto. Solo ES/EN soportados por ahora.
var culturasSoportadas = new[] { "es", "en" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(culturasSoportadas)
    .AddSupportedUICultures(culturasSoportadas));

// Cabeceras de seguridad en toda respuesta. El CSP permite 'unsafe-inline'
// en script-src/style-src porque la app usa <script>/<style> embebidos en
// las vistas (sin nonces) — igualmente bloquea carga de recursos externos
// no listados, framing (clickjacking), envío de formularios a otro origen
// y objetos embebidos, que es donde está el grueso del riesgo real.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Añade CentroId/usuario a todos los logs de la petición (mientras haya
// sesión) — imprescindible en un SaaS multi-tenant para poder filtrar los
// logs de un cliente concreto sin tener que correlacionar por IP/hora.
app.Use(async (context, next) =>
{
    var centroId = context.User.FindFirst("CentroId")?.Value;
    using (Serilog.Context.LogContext.PushProperty("CentroId", centroId))
    using (Serilog.Context.LogContext.PushProperty("Usuario", context.User.Identity?.Name))
    {
        await next();
    }
});

app.MapHealthChecks("/health");

// La ruta raíz "/" aterriza en el login. Cualquier otra ruta de un solo
// segmento ("/Residentes", "/Enfermeria"...) debe resolver a la acción
// Index de ese controlador — de ahí las dos rutas separadas: con una sola
// ruta "{controller=Account}/{action=Login}/{id?}" el valor por defecto de
// {action} sería "Login" para TODOS los controladores, y como solo
// AccountController tiene esa acción, "/Residentes" o "/Empleados" (sin
// segmento de acción) devolvían 404 en vez de su Index.
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();
