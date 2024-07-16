using Huybrechts.App.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Huybrechts.Web.Controllers;

[Route("[controller]/[action]")]
public class TenantController : Controller
{
	private readonly ApplicationUserManager _userManager;
	private readonly ApplicationTenantManager _tenantManager;

	public TenantController(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
	{
		_userManager = userManager;
		_tenantManager = tenantManager;
	}

    public async Task<IActionResult> SetTenantAsync(string tenantId, string returnUrl)
	{
		if (string.IsNullOrEmpty(tenantId))
			return LocalRedirect(returnUrl);

		var user = await _userManager.GetUserAsync(User);
		if (user is null)
			return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

		var tenant = _tenantManager.GetTenantAsync(user, tenantId);
		if (tenant is null) 
			return NotFound($"Unable to load tenant with ID '{tenantId}'.");

		HttpContext.Response.Cookies.Append("TenantID", tenantId);

		return LocalRedirect(returnUrl);
	}
}
