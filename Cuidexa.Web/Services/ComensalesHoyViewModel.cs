using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Una fila por residente activo, para el desglose de comensales de Cocina.
public class ResidenteComensalItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Habitacion { get; set; }
    public string Dieta { get; set; } = string.Empty;
}

// Una fila por empleado con turno ese día (titular o sustitución externa).
public class EmpleadoComensalItem
{
    public string Nombre { get; set; } = string.Empty;
    public RolEmpleado Rol { get; set; }
    public string BloqueHorario { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}

// Cuántas personas comen en el centro un día concreto — residentes activos
// más el personal con turno ese día — desglosado como Cocina lo necesita:
// por residente/dieta y por empleado/departamento/turno.
public class ComensalesHoyViewModel
{
    public DateOnly Fecha { get; set; }

    public List<ResidenteComensalItem> Residentes { get; set; } = new();
    public List<EmpleadoComensalItem> Empleados { get; set; } = new();

    public int TotalResidentes => Residentes.Count;
    public int TotalEmpleados => Empleados.Count;
    public int TotalComensales => TotalResidentes + TotalEmpleados;

    public List<(string Dieta, int Cantidad)> PorDieta =>
        Residentes.GroupBy(r => r.Dieta)
            .Select(g => (Dieta: g.Key, Cantidad: g.Count()))
            .OrderByDescending(x => x.Cantidad)
            .ToList();

    public List<(RolEmpleado Rol, int Cantidad)> PorDepartamento =>
        Empleados.GroupBy(e => e.Rol)
            .Select(g => (Rol: g.Key, Cantidad: g.Count()))
            .OrderByDescending(x => x.Cantidad)
            .ToList();

    // Orden fijo del día (Mañana/Tarde/Noche), no alfabético — así el
    // desglose siempre se lee en el orden natural de una jornada.
    private static readonly string[] BloquesHorarios = { "Mañana", "Tarde", "Noche" };

    public List<(string Bloque, int Cantidad)> PorTurno =>
        BloquesHorarios.Select(b => (Bloque: b, Cantidad: Empleados.Count(e => e.BloqueHorario == b))).ToList();
}
