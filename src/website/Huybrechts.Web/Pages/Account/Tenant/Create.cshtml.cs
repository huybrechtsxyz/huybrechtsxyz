using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;

        [BindProperty]
        public ApplicationTenant Input { get; set; } = new();

        [BindProperty]
        public IFormFile? PictureFile { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public CreateModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
        }

        public IActionResult OnGet()
        {
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

            var result = await _tenantManager.CreateTenantAsync(user, Input);
            if (result.IsFailed)
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;
            else
                StatusMessage = $"The tenant {result.Value.Id} has been created";

            return RedirectToPage("Index");
        }
    }
}
