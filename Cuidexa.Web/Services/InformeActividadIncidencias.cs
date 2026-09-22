using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.Services;

// Fila del informe "Actividad e incidencias". Solo cuenta avisos con
// RolDestino relleno (departamento) — las filas personales de un aviso a
// grupo (EmpleadoDestinoId) se excluyen para no contar el mismo aviso dos
// veces en el desglose por departamento.
public class FilaAvisosPorDepartamento
{
    public RolEmpleado Rol { get; set; }
    public int Total { get; set; }
    public int Urgentes { get; set; }
}

public class InformeActividadIncidencias
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public List<FilaAvisosPorDepartamento> AvisosPorDepartamento { get; set; } = new();

    // Accion: "Alta" | "Baja" | "Traslado" (EntidadTipo == "Residente" en AuditLog).
    public List<(string Accion, int Cantidad)> TendenciaResidentes { get; set; } = new();
}
