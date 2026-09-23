using Cuidexa.Web.Models;

namespace Cuidexa.Web.Services;

public class IncidenciasViewModel
{
    public List<Incidencia> Incidencias { get; set; } = new();
    public string RutaBase { get; set; } = string.Empty;

    // false para Admin: recibe y resuelve incidencias, no las reporta —
    // mismo criterio que con Avisos (ver AdminController.Incidencias).
    public bool PuedeCrear { get; set; } = true;

    // Solo Admin, por ahora (ver AdminController.FirmarIncidencia) — adjuntar
    // una firma de conformidad es opcional, no todo rol necesita gestionarla.
    public bool PuedeFirmar { get; set; }
}
