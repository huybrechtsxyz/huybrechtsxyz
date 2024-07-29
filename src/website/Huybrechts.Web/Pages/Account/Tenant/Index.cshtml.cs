using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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

        public class TeamView
        {
            [Key]
            [Required]
            [RegularExpression("^[a-z0-9]+$")]
            [StringLength(24, MinimumLength = 2)]
            [Display(Name = nameof(TeamView) + "." + nameof(Id))]
            public string Id { get; set; } = string.Empty;

            public ApplicationTenantState State { get; set; }

            [Required]
            [StringLength(256, MinimumLength = 2)]
            public string Name { get; set; } = string.Empty;

            [StringLength(512)]
            public string? Description { get; set; }

            public string? Remark { get; set; }

        }

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
