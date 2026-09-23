using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Cuidexa.Web.Controllers;

// Selector de idioma (Fase 10 — comercialización, bloque 6): fija la
// cookie de cultura estándar de ASP.NET Core y vuelve a la página de
// origen. Sin restricción de autenticación — el selector también debe
// funcionar en las pantallas de login.
[AllowAnonymous]
public class CulturaController : Controller
{
    [HttpPost]
    public IActionResult Cambiar(string cultura, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}
