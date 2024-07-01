using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenants;

public class EditModel : PageModel
{
    private readonly ApplicationUserManager userManager;
    private readonly ApplicationTenantManager tenantManager;
    private readonly ILogger<EditModel> logger;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public ApplicationTenant Input { get; set; } = new();

    [BindProperty]
    public IFormFile? PictureFile { get; set; }

    public EditModel(
        ApplicationUserManager userManager,
        ApplicationTenantManager tenantManager,
        ILogger<EditModel> logger)
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

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        if (!ModelState.IsValid)
        {
            StatusMessage = "Unexpected error when trying to update tenant.";
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

        await tenantManager.UpdateTenantAsync(user, Input);
        StatusMessage = "The tenant has been updated";
        return RedirectToPage("Index");
    }
}
