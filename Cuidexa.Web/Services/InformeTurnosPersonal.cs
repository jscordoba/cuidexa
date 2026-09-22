using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Fila por empleado del informe "Turnos y personal". HorasTrabajadas suma
// los turnos efectivos del rango (por defecto o excepción), excluyendo los
// días cedidos a otro; DiasAusencia es la única señal de ausencia que existe
// hoy en el modelo — días en que a este empleado le cubrieron su turno.
public class FilaTurnosPersonal
{
    public int EmpleadoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
    public double HorasTrabajadas { get; set; }
    public int DiasAusencia { get; set; }
    public int VacacionesDisfrutadas { get; set; }
    public int VacacionesSolicitadas { get; set; }
}

public class InformeTurnosPersonal
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public List<FilaTurnosPersonal> Filas { get; set; } = new();
}
