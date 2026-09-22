using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Usuario del sistema (personal del centro). No confundir con Residente.
public class Empleado : ITieneCentro, IProtegidaContraFuerzaBruta
{
    public int Id { get; set; }

    // "Centro de origen" del empleado — para DirectorOrganizacion es solo su
    // centro de referencia (email/auditoría), su visibilidad real se decide
    // por rol + OrganizacionId (ver CuidexaDbContext), no por esta columna.
    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
    public bool Activo { get; set; } = true;

    // Bloqueo por fuerza bruta (ver Services/BloqueoCuenta.cs).
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }

    // Solo tiene sentido cuando Rol == Profesional (fisioterapeuta, logopeda...).
    public int? EspecialidadId { get; set; }
    public Especialidad? Especialidad { get; set; }

    // Turno que se asume válido todos los días salvo excepción (reemplazo o
    // cambio aprobado) — obligatorio para los 5 roles operativos, null para
    // Admin/Dirección. Ver ITurnoService.ObtenerTurnosEfectivosAsync.
    public int? PlantillaTurnoDefectoId { get; set; }
    public PlantillaTurno? PlantillaTurnoDefecto { get; set; }
}
