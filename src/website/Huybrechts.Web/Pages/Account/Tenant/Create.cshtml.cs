using Huybrechts.App.Application;
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
        public TenantModel Input { get; set; } = new();

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
            var item =  ApplicationTenantManager.NewTenant();
            Input = new()
            {
                Id = item.Id,
                Name = item.Name,
                State = item.State,
                Description = item.Description,
                Remark = item.Remark
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

            var item = await _tenantManager.GetTenantAsync(user, Input.Id);
            if (user is null)
                return NotFound($"Unable to load tenant with ID '{Input.Id}'.");

            item.Id = Input.Id;
            item.Name = Input.Name;
            item.Description = Input.Description;
            item.Remark = Input.Remark;

            if (Request.Form.Files is not null && Request.Form.Files.Count > 0)
            { 
                IFormFile ? file = Request.Form.Files[0] ?? null;
                if (file is not null)
                {
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    item.Picture = new byte[dataStream.Length];
                    item.Picture = dataStream.ToArray();
                }
            }

            var result = await _tenantManager.CreateTenantAsync(user, item);
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
