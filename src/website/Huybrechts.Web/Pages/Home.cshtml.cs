
using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Huybrechts.Core.Platform;
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
        public ICollection<ApplicationTenant>? Tenants { get; set; }

        public ApplicationTenant? ActiveTenant => Tenants is not null && Tenants.Count == 1 ? Tenants.First() : null;

		public bool IsList() => Tenants is not null && Tenants.Count > 1;

        public bool IsDetail() => Tenants is not null && Tenants.Count == 1;
    }

	public HomeModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
		_tenantManager = tenantManager;
	}

	public async Task<IActionResult> OnGetAsync(string? tenantId = "")
    {
		Data.Tenants = null;

		var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

		if (string.IsNullOrEmpty(tenantId))
        {
			Data.Tenants = await _tenantManager.GetTenantsAsync(user, ApplicationTenantState.Active);
			return Page();
		}

        var tenant = await _tenantManager.GetTenantAsync(user, tenantId);
        if (tenant == null)
			return NotFound($"Unable to load tenant with ID '{tenantId}'.");

		Data.Tenants = [ tenant ];

		return Page();
    }
}
