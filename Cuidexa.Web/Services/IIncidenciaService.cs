using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

public interface IIncidenciaService
{
    // A quién se dirige una incidencia de este tipo (ver IncidenciaService
    // para el criterio) — usado tanto para decidir quién la ve como para el
    // push de las urgentes.
    RolEmpleado RolResponsable(TipoIncidencia tipo);

    // Bandeja de un rol operativo: las que le tocan resolver (RolResponsable)
    // más las que él mismo reportó, para poder seguir su propio caso aunque
    // el responsable sea otro departamento.
    Task<List<Incidencia>> ObtenerParaRolAsync(RolEmpleado rol);

    Task CrearAsync(TipoIncidencia tipo, int? residenteId, string titulo, string descripcion,
        GravedadIncidencia gravedad, CaracterIncidencia caracter, RolEmpleado rolOrigen, int? empleadoOrigenId);

    Task CambiarEstadoAsync(int id, EstadoIncidencia estado, string? notasResolucion, int? empleadoIdActor);
}
