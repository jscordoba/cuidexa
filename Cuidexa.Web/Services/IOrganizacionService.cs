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
}
