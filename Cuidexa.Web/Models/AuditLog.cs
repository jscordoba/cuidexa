namespace Cuidexa.Web.Models;

// Registro de auditoría: quién hizo qué y cuándo sobre una entidad del dominio.
// Deliberadamente genérico (EntidadTipo + EntidadId) para no crear una tabla de
// auditoría distinta por cada entidad que la necesite en el futuro.
public class AuditLog : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public string Accion { get; set; } = string.Empty;
    public string EntidadTipo { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public string Detalle { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
}
