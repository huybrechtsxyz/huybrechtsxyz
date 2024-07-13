using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;

        public bool AllowUpdatingTenant(ApplicationTenantState state) => ApplicationTenantManager.AllowUpdatingTenant(state);

        public bool AllowDeletingTenant(ApplicationTenantState state) => ApplicationTenantManager.AllowRemovingTenant(state);

        public IList<ApplicationTenant> Tenants { get; set; } = [];

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public IndexModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
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
            Tenants = items.OrderBy(x => x.Name).ToList();
            return Page();
        }
    }
}
