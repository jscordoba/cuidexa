using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Firma capturada (trazo en pantalla, no firma digital con validez legal
// eIDAS) que respalda un documento o una decisión concreta — consentimiento
// informado, documento de admisión, conformidad de una incidencia, entrega
// de material... ResidenteId e IncidenciaId son ambos opcionales y no se
// excluyen entre sí: no toda incidencia necesita firma de conformidad (la
// decide quien la gestiona, caso a caso), y no todo documento firmado está
// ligado a una incidencia.
public class DocumentoFirmado : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public CategoriaDocumentoFirmado Categoria { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public int? ResidenteId { get; set; }
    public Residente? Residente { get; set; }

    public int? IncidenciaId { get; set; }
    public Incidencia? Incidencia { get; set; }

    // Quién firma no es necesariamente un Empleado ni el propio Residente
    // del sistema (puede ser un familiar sin cuenta) — de ahí texto libre en
    // vez de una FK.
    public string FirmanteNombre { get; set; } = string.Empty;
    public string FirmanteRelacion { get; set; } = string.Empty;

    // PNG del trazo capturado en el canvas, en base64 — el archivo es
    // pequeño (un trazo, no una foto), así que se guarda directo en la fila
    // en vez de gestionar un archivo aparte en wwwroot/uploads.
    public string FirmaImagenBase64 { get; set; } = string.Empty;

    public DateTime FechaFirma { get; set; } = DateTime.UtcNow;

    // Quién gestionó la firma (estaba presente, dio de alta el documento).
    public int? EmpleadoRegistraId { get; set; }
    public Empleado? EmpleadoRegistra { get; set; }
}
