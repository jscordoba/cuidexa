namespace Cuidexa.Web.Services;

// Cómputo de días de vacaciones: por defecto solo cuentan los días
// laborables (sin sábado/domingo ni festivos); si la propia vacación marca
// IncluyeFinesSemanaYFestivos, se cuenta el rango completo tal cual.
public static class CalculoDiasVacaciones
{
    public static int Contar(DateOnly desde, DateOnly hasta, bool incluyeFinesSemanaYFestivos, IReadOnlySet<DateOnly> festivos)
    {
        if (incluyeFinesSemanaYFestivos)
        {
            return hasta.DayNumber - desde.DayNumber + 1;
        }

        var dias = 0;
        for (var fecha = desde; fecha <= hasta; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            if (festivos.Contains(fecha)) continue;
            dias++;
        }
        return dias;
    }
}
