
using Finbuckle.MultiTenant;
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
        public bool HasTenantInfo => TenantInfo is not null;

        public bool HasTenantList => Tenants is not null && Tenants.Count > 0;

        public TenantInfo? TenantInfo { get; set; }

        public ICollection<ApplicationTenant>? Tenants { get; set; }
    }

	public HomeModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
		_tenantManager = tenantManager;
	}

	public async Task<IActionResult> OnGetAsync()
    {
        var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        
        if (tenantInfo is null)
        {
            Data.TenantInfo = null;
            Data.Tenants = await _tenantManager.GetTenantsAsync(user, ApplicationTenantState.Active);
            return Page();
        }

        Data.TenantInfo = tenantInfo;
        Data.Tenants = null;

		return Page();
    }
}
