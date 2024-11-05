using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Huybrechts.Landing.Web.Controllers;

[Route("[controller]/[action]")]
public class ApplicationController : Controller
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
}
