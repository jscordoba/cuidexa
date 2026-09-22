namespace Cuidexa.Web.Models;

// Tabla intermedia N:M entre Residente y Patologia. A diferencia de
// ResidenteAlergia lleva Observaciones porque aquí sí hace falta contexto
// clínico específico del residente (cifrado en reposo, ver CuidexaDbContext).
public class ResidentePatologia : ITieneCentro
{
    // Denormalizado a partir de Residente.CentroId (ver ResidenteDieta).
    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public int PatologiaId { get; set; }
    public Patologia? Patologia { get; set; }

    public string? Observaciones { get; set; }
}
