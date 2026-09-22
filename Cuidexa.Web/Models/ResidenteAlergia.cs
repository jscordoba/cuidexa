namespace Cuidexa.Web.Models;

// Tabla intermedia N:M entre Residente y Alergia.
public class ResidenteAlergia : ITieneCentro
{
    // Denormalizado a partir de Residente.CentroId (ver ResidenteDieta).
    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public int AlergiaId { get; set; }
    public Alergia? Alergia { get; set; }
}
