using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenants;

public class ActivateModel : PageModel
{
    private readonly ApplicationUserManager userManager;
    private readonly ApplicationTenantManager tenantManager;
    private readonly ILogger<ActivateModel> logger;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Input { get; set; } = new();

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public ActivateModel(
        ApplicationUserManager userManager,
        ApplicationTenantManager tenantManager,
        ILogger<ActivateModel> logger)
    {
        this.userManager = userManager;
        this.tenantManager = tenantManager;
        this.logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        var item = await tenantManager.GetTenantAsync(user, id);
        if (item is null || string.IsNullOrEmpty(item.Id))
            return NotFound();
        Input = item;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        try
        { 
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

            var item = await tenantManager.GetTenantAsync(user, id);
            if (item is null || string.IsNullOrEmpty(item.Id))
                return NotFound();

            await tenantManager.SetActiveTenantAsync(user, item);
            StatusMessage = "The tenant has been marked for activation";
        }
        catch (ApplicationException ex)
        {
            StatusMessage = ex.Message;
        }
        return RedirectToPage("Index");
    }
}
