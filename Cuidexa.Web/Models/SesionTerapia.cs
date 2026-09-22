using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Sesión de terapia (fisioterapia, logopedia...) de un residente con un
// profesional concreto. La especialidad no se duplica aquí: se lee a
// través de Empleado.Especialidad. Observaciones (la evolución) va cifrada
// en reposo, igual que el resto de información clínica.
public class SesionTerapia : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public int EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    // Hora local del centro tal como la escribe el profesional, no una hora
    // UTC real convertida — se guarda con Kind=Utc solo porque Npgsql lo exige
    // para "timestamp with time zone" (ver ProfesionalesService.ProgramarSesionAsync).
    // Por eso NO debe mostrarse con .ToLocalTime() en las vistas: ya está en
    // hora local, aplicar la conversión la desplazaría de nuevo.
    public DateTime FechaHora { get; set; }
    public EstadoSesion Estado { get; set; } = EstadoSesion.Programada;
    public string? Observaciones { get; set; }
}
