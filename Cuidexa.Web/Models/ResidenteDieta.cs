namespace Cuidexa.Web.Models;

// Historial de dietas del residente. La dieta activa es la fila con FechaFin == null.
public class ResidenteDieta : ITieneCentro
{
    public int Id { get; set; }

    // Denormalizado a partir de Residente.CentroId — necesario porque el
    // filtro global de EF Core no se encadena automáticamente a través de
    // una consulta directa a esta tabla (ver CuidexaDbContext).
    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public int DietaId { get; set; }
    public Dieta? Dieta { get; set; }

    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string? Motivo { get; set; }
}
