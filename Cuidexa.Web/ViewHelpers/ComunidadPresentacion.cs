using Cuidexa.Web.Models.Enums;

namespace Cuidexa.Web.ViewHelpers;

// Etiqueta legible + código de comunidad que usa calendariosnacionales.com
// en sus URLs (ver IFestivosApiService) — un único sitio para no
// desincronizar el desplegable de la UI del slug real de la API.
public static class ComunidadPresentacion
{
    public static (string Etiqueta, string SlugApi) Obtener(ComunidadAutonoma c) => c switch
    {
        ComunidadAutonoma.Andalucia => ("Andalucía", "and"),
        ComunidadAutonoma.Aragon => ("Aragón", "ara"),
        ComunidadAutonoma.Asturias => ("Asturias", "ast"),
        ComunidadAutonoma.Canarias => ("Canarias", "can"),
        ComunidadAutonoma.Cantabria => ("Cantabria", "cnt"),
        ComunidadAutonoma.CastillaYLeon => ("Castilla y León", "cyl"),
        ComunidadAutonoma.CastillaLaMancha => ("Castilla-La Mancha", "clm"),
        ComunidadAutonoma.Cataluna => ("Cataluña", "cat"),
        ComunidadAutonoma.Ceuta => ("Ceuta", "ceu"),
        ComunidadAutonoma.Madrid => ("Comunidad de Madrid", "mad"),
        ComunidadAutonoma.Navarra => ("Comunidad Foral de Navarra", "nav"),
        ComunidadAutonoma.ComunidadValenciana => ("Comunitat Valenciana", "val"),
        ComunidadAutonoma.Extremadura => ("Extremadura", "ext"),
        ComunidadAutonoma.Galicia => ("Galicia", "gal"),
        ComunidadAutonoma.IllesBalears => ("Illes Balears", "bal"),
        ComunidadAutonoma.LaRioja => ("La Rioja", "rio"),
        ComunidadAutonoma.Melilla => ("Melilla", "mel"),
        ComunidadAutonoma.PaisVasco => ("País Vasco", "eus"),
        ComunidadAutonoma.Murcia => ("Región de Murcia", "mur"),
        _ => (c.ToString(), "")
    };
}
