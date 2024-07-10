using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Manage;

public class TenantCardModel : PageModel
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationTenantManager _tenantManager;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Tenant{ get; set; } = new();

    [BindProperty]
    public List<ApplicationRole> Roles { get; set; }

    [BindProperty]
    public List<ApplicationUserRole> UserRoles { get; set; }

    public TenantCardModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
        _tenantManager = tenantManager;
        Roles = [];
        UserRoles = [];
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound($"Unable to load tentant with ID '{id}'.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var item = await _tenantManager.GetTenantAsync(user, id);
        if (item is null || string.IsNullOrEmpty(item.Id))
            return NotFound($"Unable to load tentant with ID '{id}'.");

        Tenant = item;
        Roles = await _tenantManager.GetTenantRolesAsync(user, id);
        UserRoles = await _tenantManager.GetTenantUserRolesAsync(user, id);

        return Page();
    }
}