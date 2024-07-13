using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;

        [BindProperty]
        public ApplicationTenant Input { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public DeleteModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
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

            var result = await _tenantManager.DeleteTenantAsync(user, Input);
            if (result.IsFailed)
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;
            else
                StatusMessage = $"The tenant {result.Value.Id} has been marked for deletion";

            return RedirectToPage("Index");
        }
    }
}
