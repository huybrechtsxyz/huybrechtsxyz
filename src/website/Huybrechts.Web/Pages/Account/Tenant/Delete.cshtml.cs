using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;
        private readonly IStringLocalizer<DeleteModel> _localizer;

        [BindProperty]
        public ApplicationTenant Input { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public DeleteModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager, IStringLocalizer<DeleteModel> localizer)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound($"Unable to load team with ID '{id}'.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var item = await _tenantManager.GetTenantAsync(user, id);
            if (item is null || string.IsNullOrEmpty(item.Id))
                return NotFound($"Unable to load team with ID '{id}'.");
            else
                Input = item;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var result = await _tenantManager.DeleteTenantAsync(user, Input);
            if (!result.IsFailed)
            {
                var message = _localizer["The team {0} has been marked for deletion"];
                StatusMessage = message.Value.Replace("{0}", result.Value.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;

            return RedirectToPage("Index");
        }
    }
}
