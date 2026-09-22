namespace Cuidexa.Web.Models;

// Día festivo del centro (Navidad, fiestas locales...). Solo existe para que
// el cómputo de días de vacaciones pueda excluirlo — no tiene relación con
// PlantillaTurno.TipoDia, que describe un tipo de franja horaria, no una
// fecha concreta del calendario.
public class Feriado : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public DateOnly Fecha { get; set; }
    public string? Nombre { get; set; }
}
