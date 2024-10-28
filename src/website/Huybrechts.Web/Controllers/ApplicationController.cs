using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace Huybrechts.Web.Controllers;

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

    public void SetTheme(string theme)
    {
        HttpContext.Session.SetString("data-bs-theme", (theme == "on" ? "dark" : "light"));
    }

    [HttpGet("~/connect/authorize")]
    public IActionResult Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (!User.Identity.IsAuthenticated)
        {
            // Challenge the user if they're not authenticated
            return Challenge();
        }

        // Sign in the user and issue an authorization code
        return SignIn(User, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    public IActionResult Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request.IsAuthorizationCodeGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, "user-id");

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(new[] { OpenIddictConstants.Scopes.OpenId, OpenIddictConstants.Scopes.Profile });

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest("Invalid grant type.");
    }
}
