using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Sesión de "tablet compartida" por departamento (Fase 6) — no representa a
// un empleado concreto, solo consulta en modo lectura. Se autentica con un
// esquema de cookie separado del de Empleado (ver Program.cs) para que nunca
// pueda alcanzar las acciones de escritura de los controladores operativos,
// aunque comparta el mismo claim de Rol.
public class CuentaDispositivo : ITieneCentro, IProtegidaContraFuerzaBruta
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Restringido por CuentaDispositivoService a Cocina/Enfermeria/Auxiliar.
    public RolEmpleado Rol { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Bloqueo por fuerza bruta (ver Services/BloqueoCuenta.cs).
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }

    // Solo se actualiza al iniciar sesión, no en cada auto-refresh del
    // dashboard — evita escrituras cada 60s desde una tablet siempre encendida.
    public DateTime? UltimoAcceso { get; set; }
}
