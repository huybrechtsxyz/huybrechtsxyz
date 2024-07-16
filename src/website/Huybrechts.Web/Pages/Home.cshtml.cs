
using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages;

[Authorize]
public class HomeModel : PageModel
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationTenantManager _tenantManager;

    [BindProperty]
    public HomeModelView Data { get; set; } = new();

    public class HomeModelView
    {
        public ICollection<ApplicationTenant> Tenants { get; set; } = [];
    }

	public HomeModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
		_tenantManager = tenantManager;
	}

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var items = await _tenantManager.GetTenantsAsync(user);
        Data.Tenants = items.OrderBy(x => x.Name).ToList();
        return Page();
    }
}
