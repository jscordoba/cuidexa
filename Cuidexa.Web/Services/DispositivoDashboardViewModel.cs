using Cuidexa.Web.Models;
using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Resumen de solo lectura para el dashboard de una tablet compartida —
// ensamblado a partir de servicios ya existentes (avisos, turnos, comensales,
// agenda de enfermería), nunca queries nuevas duplicadas.
public class DispositivoDashboardViewModel
{
    public string NombreDispositivo { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }

    public int AvisosPendientesCount { get; set; }
    public List<EventoDistribucion> AvisosPendientes { get; set; } = new();

    public List<TurnoEfectivo> PersonalHoy { get; set; } = new();

    // Solo Cocina.
    public ComensalesHoyViewModel? ComensalesHoy { get; set; }

    // Solo Enfermería.
    public int? MedicacionPendienteHoy { get; set; }
}
