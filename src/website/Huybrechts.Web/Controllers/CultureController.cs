using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Huybrechts.Web.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (culture != null)
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(culture, culture)));
        }

        return LocalRedirect(returnUrl);
    }

    public void SetTheme(string theme)
    {
        HttpContext.Session.SetString("data-bs-theme", (theme == "on" ? "dark" : "light"));
    }
}
