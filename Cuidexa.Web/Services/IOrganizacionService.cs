using Cuidexa.Web.Models;
using Microsoft.AspNetCore.Http;

namespace Cuidexa.Web.Services;

// Marca compartida por todos los Centros de la Organización de la sesión
// actual (Fase 9) — nombre, logo, color de acento. Los datos físicos de un
// Centro concreto viven en ICentroService.
public interface IOrganizacionService
{
    Task<Organizacion> ObtenerAsync();
    Task ActualizarMarcaAsync(string? nombre, string? colorAcento, int? actorId);

    // null en archivo: sin cambios (el formulario no incluyó un fichero nuevo).
    Task ActualizarLogoAsync(IFormFile? archivo, int? actorId);

    // Variantes para SuperAdmin (ver SuperAdminController): sin sesión de
    // tenant propia, así que reciben el id de la organización explícito en
    // vez de resolverlo por ITenantContext.OrganizacionId. Cubren tanto una
    // organización con varios centros ("Fundación X") como una con uno solo
    // — la marca es de la Organización, el número de Centros que tenga no
    // cambia nada de esto.
    Task<Organizacion> ObtenerPorIdAsync(int organizacionId);
    Task ActualizarMarcaComoSuperAdminAsync(int organizacionId, string? nombre, string? colorAcento);
    Task ActualizarLogoComoSuperAdminAsync(int organizacionId, IFormFile? archivo);
}
