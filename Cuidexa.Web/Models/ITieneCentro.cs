namespace Cuidexa.Web.Models;

// Implementada por toda entidad aislada por Centro físico (Fase 9) — permite
// a CuidexaDbContext configurar la FK y el filtro global de aislamiento una
// sola vez, de forma genérica, en vez de repetir la misma configuración
// prácticamente idéntica para cada una de las ~19 tablas afectadas.
public interface ITieneCentro
{
    int CentroId { get; set; }
    Centro? Centro { get; set; }
}
