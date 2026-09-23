namespace Cuidexa.Web.Models.Enums;

// Distingue si una incidencia necesita que alguien la resuelva o si es solo
// para dejar constancia (no toda incidencia operativa/de instalaciones
// implica una acción pendiente). Una Informativa se crea directamente en
// Estado.Resuelta (ver IncidenciaService.CrearAsync) — nunca aparece como
// abierta ni dispara push, aunque queda igualmente en el historial del
// centro para quien necesite consultarla.
public enum CaracterIncidencia
{
    RequiereSeguimiento,
    Informativa
}
