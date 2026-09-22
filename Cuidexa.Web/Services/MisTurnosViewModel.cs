using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Datos que necesita la pantalla "Mis turnos", compartida por los 5 roles
// operativos (Cocina, Auxiliares, Enfermería, Profesionales, Limpieza) —
// una sola vista en Views/Shared en vez de cinco copias casi idénticas.
public class MisTurnosViewModel
{
    // Turno que se asume todos los días salvo excepción. Null solo si Admin
    // no lo configuró (no debería ocurrir para un rol operativo).
    public PlantillaTurno? PlantillaDefecto { get; set; }

    // Turnos de excepción (reemplazos o cambios ya aprobados) donde este
    // empleado es quien cubre o a quien le cubrieron su turno — próximos.
    public List<Turno> TurnosEspeciales { get; set; } = new();
    public int EmpleadoIdPropio { get; set; }

    public List<Empleado> Companeros { get; set; } = new();
    public List<CambioTurno> MisCambios { get; set; } = new();

    public List<Vacacion> MisVacaciones { get; set; } = new();

    public RolEmpleado RolPropio { get; set; }

    // Prefijo de ruta del rol actual (ej. "Enfermeria"), para construir las
    // URLs de los formularios sin acoplar la vista compartida a un controlador.
    public string RutaBase { get; set; } = string.Empty;
}
