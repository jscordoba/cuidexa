using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Datos para el formulario "Nuevo aviso", compartido por los 5 controladores
// operativos (mismo patrón que MisTurnosViewModel) — cada uno solo aporta su
// propio rol y a qué ruta pertenece.
public class CrearAvisoViewModel
{
    public List<Residente> Residentes { get; set; } = new();
    public RolEmpleado RolOrigen { get; set; }
    public string RutaBase { get; set; } = string.Empty;

    // Grupos de notificación activos (Fase 7) — destino opcional alternativo
    // a marcar departamentos adicionales.
    public List<GrupoNotificacion> GruposDisponibles { get; set; } = new();

    // Departamentos que se pueden marcar como "adicionales" — todos los
    // operativos excepto el propio (que ya va obligado) y Admin (que también
    // va obligado, pero no como opción marcable).
    public List<RolEmpleado> DepartamentosDisponibles =>
        Enum.GetValues<RolEmpleado>()
            .Where(r => r != RolOrigen && r != RolEmpleado.Admin && r != RolEmpleado.Direccion)
            .ToList();
}
