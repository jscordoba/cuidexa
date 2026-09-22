using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Centro físico (un edificio/sede concreta) — el límite real de aislamiento
// de datos operativos. Pertenece siempre a una Organizacion, que puede tener
// varios Centros (ej. una fundación con varias sedes).
public class Centro
{
    public int Id { get; set; }

    public int OrganizacionId { get; set; }
    public Organizacion? Organizacion { get; set; }

    // Nombre propio del edificio (ej. "Centro Tres Cantos") — distingue
    // centros hermanos de la misma organización en listados de SuperAdmin/
    // Director de Organización. La marca visible en login/menú la da
    // Organizacion.Nombre, no este campo.
    public string Nombre { get; set; } = string.Empty;

    // Código que el personal teclea al iniciar sesión para identificar a qué
    // centro pertenece (ver AccountController/TabletController).
    public string Codigo { get; set; } = string.Empty;

    public ComunidadAutonoma? ComunidadAutonoma { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? ResponsableNombre { get; set; }
    public string? ResponsableCargo { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
