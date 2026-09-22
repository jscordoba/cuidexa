using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Datos físicos del Centro de la sesión actual (Fase 9) — dirección,
// contacto, responsable, comunidad autónoma. La marca (nombre, logo, color)
// vive en IOrganizacionService: es un dato compartido por todos los Centros
// de la misma Organización, no de un edificio concreto.
public interface ICentroService
{
    Task<Centro> ObtenerAsync();

    // Para DirectorOrganizacion: los centros entre los que puede elegir al
    // dar de alta algo nuevo (residente, empleado, turno externo...) que no
    // hereda el centro de ninguna otra fila ya existente.
    Task<List<Centro>> ObtenerCentrosDeMiOrganizacionAsync();

    Task ActualizarContactoAsync(string nombre, string? direccion, string? telefono, string? email,
        string? responsableNombre, string? responsableCargo, int? actorId);

    Task ActualizarComunidadAsync(ComunidadAutonoma? comunidad, int? actorId);
}
