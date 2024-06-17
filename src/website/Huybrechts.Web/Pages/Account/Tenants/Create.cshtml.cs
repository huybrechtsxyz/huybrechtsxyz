using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenants;

public class CreateModel : PageModel
{
    private readonly ApplicationUserManager userManager;
    private readonly ApplicationTenantManager tenantManager;
    private readonly ILogger<IndexModel> logger;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Input { get; set; } = new();

    public CreateModel(
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

        Initialize();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            StatusMessage = "Unexpected error when trying to create tenant.";
            return Page();
        }

        await tenantManager.CreateTenantAsync(user, Input);
        StatusMessage = "The tenant has been create";
        return RedirectToPage("Index");
    }

    private void Initialize()
    {
        Input = ApplicationTenantManager.NewTenant();
    }
}
