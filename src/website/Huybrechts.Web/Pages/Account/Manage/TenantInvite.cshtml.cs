using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        [DisplayName("Tenant ID")]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        [DisplayName("Tenant Role")]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
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
            var roles = await _tenantManager.GetTenantRolesAsync(user, Input.TenantId) ?? [];
            Roles = [.. roles.OrderBy(o => o.Label)];
            
            StatusMessage = "Unexpected error when trying to create tenant.";
            return Page();
        }

        string tenantId = Input.TenantId;
        var newUsers = Input.Users?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray() ?? [];
        var messages = await _tenantManager.AddUsersToTenant(user, Input.TenantId, Input.RoleId, newUsers);
        StatusMessage = string.Join(Environment.NewLine, messages);
        return RedirectToPage("TenantCard", "", new { id = tenantId });
    }
}
