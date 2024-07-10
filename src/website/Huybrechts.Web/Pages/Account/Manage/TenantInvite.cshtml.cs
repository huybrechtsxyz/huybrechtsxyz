using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Manage;

public class TenantInviteModel : PageModel
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationTenantManager _tenantManager;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public Invitation Input { get; set; } = new();

    [BindProperty]
    public List<ApplicationRole> Roles { get; set; } = [];

    public class Invitation
    {
        public string TenantId { get; set; } = string.Empty;

        public string RoleId { get; set; } = string.Empty;

        public string? Users { get; set; }
    }

    public TenantInviteModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
        _tenantManager = tenantManager;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var tenant = await _tenantManager.GetTenantAsync(user, id);
        if (tenant == null)
            return NotFound($"Unable to load tenant with ID '{id}'.");

        var roles = await _tenantManager.GetTenantRolesAsync(user, id) ?? [];
        Roles = [.. roles.OrderBy(o => o.Label)];

        Input = new()
        {
            TenantId = tenant.Id,
            RoleId = string.Empty,
            Users = string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        if (!ModelState.IsValid)
        {
            StatusMessage = "Unexpected error when trying to create tenant.";
            return Page();
        }

        List<string> newUsers = Input.Users?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? [];

        await _tenantManager.AddUsersToTenantAsync(user, Input.TenantId, Input.RoleId, newUsers);
        StatusMessage = "The tenant invate(s) has been created";
        return RedirectToPage("TenantCard");
    }
}
