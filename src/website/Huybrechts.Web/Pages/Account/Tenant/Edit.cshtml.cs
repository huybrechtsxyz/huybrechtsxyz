using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class EditModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;

        public bool AllowEnablingTenant() => ApplicationTenantManager.AllowEnablingTenant(Input.State);

        public bool AllowDisablingTenant() => ApplicationTenantManager.AllowDisablingTenant(Input.State);

        [BindProperty]
        public ApplicationTenant Input { get; set; } = new();

        [BindProperty]
        public IFormFile? PictureFile { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public ICollection<ApplicationRole> Roles { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public class InvitationModel
        {
            [Required]
            [DisplayName("Tenant ID")]
            public string TenantId { get; set; } = string.Empty;

            [Required]
            [DisplayName("Tenant Role")]
            public string RoleId { get; set; } = string.Empty;

            [Required]
            [MinLength(3)]
            public string? Users { get; set; }
        }

        public EditModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
            Roles = [];
            UserRoles = [];
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
            
            Input = item;
            Roles = await _tenantManager.GetTenantRolesAsync(user, item.Id);
            UserRoles = await _tenantManager.GetTenantUserRolesAsync(user, item.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            Roles = await _tenantManager.GetTenantRolesAsync(user, Input.Id);
            UserRoles = await _tenantManager.GetTenantUserRolesAsync(user, Input.Id);

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

            var result = await _tenantManager.UpdateTenantAsync(user, Input);
            if (result.IsFailed)
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;
            else
                StatusMessage = $"The tenant {result.Value.Id} has been updated";

            return Page();
        }

        public async Task<IActionResult> OnPostEnableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var result = await _tenantManager.EnableTenantAsync(user, Input);
            if (result.IsFailed)
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;
            else
                StatusMessage = $"The tenant {Input.Id} has been set activation";

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDisableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var result = await _tenantManager.DisableTenantAsync(user, Input);
            if (result.IsFailed)
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;
            else
                StatusMessage = $"The tenant {Input.Id} has been disabled";

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId, string roleId, string tenantId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var result = await _tenantManager.RemoveUserFromTenantAsync(user, userId, roleId, tenantId);
            StatusMessage = string.Empty;
            foreach (var item in result.Reasons)
            {
                StatusMessage += !string.IsNullOrEmpty(StatusMessage) ? Environment.NewLine : string.Empty;
                StatusMessage += item.Message;
            }

            return RedirectToPage("Edit", "", new { id = tenantId });
        }
    }
}
