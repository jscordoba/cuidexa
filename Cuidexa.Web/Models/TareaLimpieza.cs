using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Tarea de limpieza: o bien de una habitación concreta (HabitacionId) o de
// una zona común sin catálogo propio (Zona, texto libre — comedor, salón,
// pasillo...). No lleva datos de residentes ni cifrado: es información
// puramente operativa, igual que los avisos de Cocina/Auxiliares.
public class TareaLimpieza : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int? HabitacionId { get; set; }
    public Habitacion? Habitacion { get; set; }
    public string? Zona { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCompletada { get; set; }
    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }
}
