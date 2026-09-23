using Cuidexa.Web.Data;
using Cuidexa.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Cuidexa.Web.Services;

public class OrganizacionService : IOrganizacionService
{
    // Sin .svg a propósito: un SVG puede llevar <script> embebido y el
    // archivo queda accesible públicamente (sin autenticación) bajo
    // /uploads — quien navegue directamente a esa URL lo ejecutaría en el
    // origen de la app. png/jpg/jpeg son datos de imagen puros, sin ese riesgo.
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg" };
    private const long TamanoMaximoBytes = 2 * 1024 * 1024;

    private readonly CuidexaDbContext _db;
    private readonly IAuditService _auditoria;
    private readonly IWebHostEnvironment _entorno;
    private readonly ITenantContext _tenant;

    public OrganizacionService(CuidexaDbContext db, IAuditService auditoria, IWebHostEnvironment entorno, ITenantContext tenant)
    {
        _db = db;
        _auditoria = auditoria;
        _entorno = entorno;
        _tenant = tenant;
    }

    // Organizacion nunca lleva filtro global — se acota explícitamente a la
    // de la sesión actual, igual que ICentroService.ObtenerAsync().
    public async Task<Organizacion> ObtenerAsync() =>
        await ObtenerPorIdAsync(_tenant.OrganizacionId);

    public async Task<Organizacion> ObtenerPorIdAsync(int organizacionId) =>
        await _db.Organizaciones.FindAsync(organizacionId)
            ?? throw new InvalidOperationException("Organización no encontrada.");

    public Task ActualizarMarcaAsync(string? nombre, string? colorAcento, int? actorId) =>
        ActualizarMarcaInternoAsync(_tenant.OrganizacionId, nombre, colorAcento, actorId);

    // SuperAdmin no tiene OrganizacionId de sesión (ver ITenantContext) — de
    // ahí el id explícito. Sin auditoría por actorId (SuperAdmin no es un
    // Empleado): igual que SuperAdminController.CrearOrganizacion/CrearCentro,
    // que tampoco auditan la acción con IAuditService.
    public Task ActualizarMarcaComoSuperAdminAsync(int organizacionId, string? nombre, string? colorAcento) =>
        ActualizarMarcaInternoAsync(organizacionId, nombre, colorAcento, actorId: null);

    private async Task ActualizarMarcaInternoAsync(int organizacionId, string? nombre, string? colorAcento, int? actorId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre de la organización es obligatorio.");
        }

        if (!string.IsNullOrWhiteSpace(colorAcento) && !System.Text.RegularExpressions.Regex.IsMatch(colorAcento, "^#[0-9a-fA-F]{6}$"))
        {
            throw new InvalidOperationException("El color de acento debe ser un código hexadecimal válido (ej. #4f46e5).");
        }

        var organizacion = await ObtenerPorIdAsync(organizacionId);
        organizacion.Nombre = nombre;
        organizacion.ColorAcento = string.IsNullOrWhiteSpace(colorAcento) ? null : colorAcento;
        await _db.SaveChangesAsync();

        if (actorId.HasValue)
        {
            await _auditoria.RegistrarAsync(actorId, "ConfiguracionActualizada", "Organizacion", organizacion.Id,
                "Datos de marca de la organización actualizados.");
        }
    }

    public Task ActualizarLogoAsync(IFormFile? archivo, int? actorId) =>
        ActualizarLogoInternoAsync(_tenant.OrganizacionId, archivo, actorId);

    public Task ActualizarLogoComoSuperAdminAsync(int organizacionId, IFormFile? archivo) =>
        ActualizarLogoInternoAsync(organizacionId, archivo, actorId: null);

    private async Task ActualizarLogoInternoAsync(int organizacionId, IFormFile? archivo, int? actorId)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return;
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            throw new InvalidOperationException("El logo no puede superar 2MB.");
        }

        var extension = Path.GetExtension(archivo.FileName);
        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("El logo debe ser .png, .jpg o .jpeg.");
        }

        var organizacion = await ObtenerPorIdAsync(organizacionId);

        // Carpeta por organización (no compartida): más fácil de razonar y de
        // limpiar si una organización se da de baja, aunque el nombre GUID ya
        // evitaba colisiones por sí solo.
        var carpeta = Path.Combine(_entorno.WebRootPath, "uploads", $"org-{organizacion.Id}");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"logo-{Guid.NewGuid():N}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
        await using (var destino = File.Create(rutaCompleta))
        {
            await archivo.CopyToAsync(destino);
        }

        if (!string.IsNullOrWhiteSpace(organizacion.LogoRuta))
        {
            var rutaAnterior = Path.Combine(_entorno.WebRootPath, organizacion.LogoRuta.TrimStart('/'));
            if (File.Exists(rutaAnterior))
            {
                File.Delete(rutaAnterior);
            }
        }

        organizacion.LogoRuta = $"/uploads/org-{organizacion.Id}/{nombreArchivo}";
        await _db.SaveChangesAsync();

        if (actorId.HasValue)
        {
            await _auditoria.RegistrarAsync(actorId, "ConfiguracionActualizada", "Organizacion", organizacion.Id,
                "Logo de la organización actualizado.");
        }
    }
}
