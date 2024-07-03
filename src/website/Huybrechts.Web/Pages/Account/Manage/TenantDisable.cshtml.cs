using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Manage;

public class TenantDisableModel : PageModel
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationTenantManager _tenantManager;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Input { get; set; } = new();

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public TenantDisableModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
        _tenantManager = tenantManager;
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
        else
            Input = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        if (!ModelState.IsValid)
        {
            StatusMessage = "Unexpected error when trying to disable tenant.";
            return Page();
        }

        await _tenantManager.DisableTenantAsync(user, Input);
        StatusMessage = "The tenant has been disabled.";
        return RedirectToPage("Tenants");
    }
}
