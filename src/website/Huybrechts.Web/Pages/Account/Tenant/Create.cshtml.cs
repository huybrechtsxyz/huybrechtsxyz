using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;
        private readonly IStringLocalizer<CreateModel> _localizer;

        [BindProperty]
        public ApplicationTenant Input { get; set; } = new();

        [BindProperty]
        public IFormFile? PictureFile { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public CreateModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager, IStringLocalizer<CreateModel> localizer)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
            _localizer = localizer;
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

            if(Request.Form.Files is not null && Request.Form.Files.Count > 0)
            { 
                IFormFile ? file = Request.Form.Files[0] ?? null;
                if (file is not null)
                {
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    Input.Picture = new byte[dataStream.Length];
                    Input.Picture = dataStream.ToArray();
                }
            }

            var result = await _tenantManager.CreateTenantAsync(user, Input);
            if (!result.IsFailed)
            {
                var message = _localizer["The team {0} has been created"];
                StatusMessage = message.Value.Replace("{0}", result.Value.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;

            return RedirectToPage("Index");
        }
    }
}
