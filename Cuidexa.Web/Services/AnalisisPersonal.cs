using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Predicción local basada en heurística estadística sobre turnos históricos
// (promedio de personal distinto por día de la semana + rol de las últimas
// semanas) — no es un modelo de IA/ML ni llama a ningún servicio externo.
// Ver AnalisisService.PredecirPersonalNecesarioAsync.
public class FilaPrediccionPersonal
{
    public DateOnly Fecha { get; set; }
    public RolEmpleado Rol { get; set; }
    public int PersonalProgramado { get; set; }
    public double PersonalRecomendado { get; set; }
    public bool PosibleFaltaPersonal { get; set; }
}

public class AnalisisPersonal
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public List<FilaPrediccionPersonal> Filas { get; set; } = new();
}
