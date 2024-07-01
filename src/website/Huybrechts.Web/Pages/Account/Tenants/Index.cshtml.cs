using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenants;

public class IndexModel : PageModel
{
    private readonly ApplicationUserManager userManager;
    private readonly ApplicationTenantManager tenantManager;
    private readonly ILogger<IndexModel> logger;

    public IList<ApplicationTenant> Tenants { get; set; } = [];

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public List<ApplicationTenant> ApplicationTenants { get; set; } = [];
    }

    public IndexModel(
        ApplicationUserManager userManager,
			ApplicationTenantManager tenantManager,
			ILogger<IndexModel> logger)
		{
        this.userManager = userManager;
        this.tenantManager = tenantManager;
        this.logger = logger;
    }

	public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        Tenants = await tenantManager.GetTenantsAsync(user);

        return Page();
    }
}
