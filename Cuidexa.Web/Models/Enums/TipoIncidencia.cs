namespace Cuidexa.Web.Models.Enums;

// A qué/quién afecta la incidencia — determina el rol responsable de
// resolverla (ver Services/IncidenciaService.RolResponsable) y si lleva
// ResidenteId. Deliberadamente separado del sistema de Avisos (Fase 7):
// una incidencia necesita seguimiento hasta resolverse, un aviso solo
// leído/no leído.
public enum TipoIncidencia
{
    Residente,
    Personal,
    Instalaciones
}
