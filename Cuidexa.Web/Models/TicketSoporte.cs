using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Models;

// Canal de soporte para clientes, autocontenido: no depende de email/SMTP
// (que la app no tiene configurado). Un empleado reporta desde dentro de
// Cuidexa, un SuperAdmin lo ve y responde desde su propio panel — la
// respuesta queda aquí mismo, visible para el centro que lo reportó. Ver
// COMERCIALIZACION.md, bloque 3.
public class TicketSoporte : ITieneCentro
{
    public int Id { get; set; }

    public int CentroId { get; set; }
    public Centro? Centro { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public int? EmpleadoId { get; set; }
    public Empleado? Empleado { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoTicketSoporte Estado { get; set; } = EstadoTicketSoporte.Abierto;

    public string? Respuesta { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public int? RespondidoPorSuperAdminId { get; set; }
    public SuperAdmin? RespondidoPorSuperAdmin { get; set; }
}
