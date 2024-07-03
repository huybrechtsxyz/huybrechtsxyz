using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Manage;

public class TenantCreateModel : PageModel
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationTenantManager _tenantManager;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Input { get; set; } = new();

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public TenantCreateModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
    {
        _userManager = userManager;
        _tenantManager = tenantManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        Input = ApplicationTenantManager.NewTenant();
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

        IFormFile? file = Request.Form.Files.FirstOrDefault();
        if (file is not null)
        {
            using var dataStream = new MemoryStream();
            await file.CopyToAsync(dataStream);
            Input.Picture = new byte[dataStream.Length];
            Input.Picture = dataStream.ToArray();
        }

        await _tenantManager.CreateTenantAsync(user, Input);
        StatusMessage = "The tenant has been created";
        return RedirectToPage("Tenants");
    }
}
