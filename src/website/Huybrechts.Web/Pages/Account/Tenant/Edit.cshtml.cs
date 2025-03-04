using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class EditModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;
        private readonly IStringLocalizer<EditModel> _localizer;

        public bool AllowEnablingTenant() => ApplicationTenantManager.AllowEnablingTenant(Input.State);

        public bool AllowDisablingTenant() => ApplicationTenantManager.AllowDisablingTenant(Input.State);

        public bool AllowDefaultsTenant() => ApplicationTenantManager.AllowDisablingTenant(Input.State) 
            || ApplicationTenantManager.AllowDisablingTenant(Input.State);

        [BindProperty]
        public TenantModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? PictureFile { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public ICollection<ApplicationRole> Roles { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public EditModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager, IStringLocalizer<EditModel> localizer)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
            _localizer = localizer;
            Roles = [];
            UserRoles = [];
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

            Input = new TenantModel()
            {
                Id = item.Id,
                Name = item.Name,
                State = item.State,
                Description = item.Description,
                Remark = item.Remark
            };
            if (item.Picture is not null && item.Picture.Length > 0)
            {
                Input.Picture = new byte[item.Picture.Length];
                Input.Picture = [.. item.Picture];
            }

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
                StatusMessage = _localizer["Unexpected error when trying to update team."];
                return Page();
            }

            var item = await _tenantManager.GetTenantAsync(user, Input.Id);
            if (item is null)
                return NotFound($"Unable to load team with ID '{Input.Id}'.");

            item.Name = Input.Name;
            item.Description = Input.Description;
            item.Remark = Input.Remark;

            if (Request.Form.Files is not null && Request.Form.Files.Count > 0)
            {
                IFormFile? file = Request.Form.Files[0];
                if (file is not null)
                {
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    item.Picture = new byte[dataStream.Length];
                    item.Picture = dataStream.ToArray();
                }
            }

            var result = await _tenantManager.UpdateTenantAsync(user, item);
            if (!result.IsFailed)
            {
                var message = _localizer["The team {0} has been updated"];
                StatusMessage = message.Value.Replace("{0}", Input.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostEnableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var item = await _tenantManager.GetTenantAsync(user, Input.Id);
            if (item is null)
                return NotFound($"Unable to load team with ID '{Input.Id}'.");

            var result = await _tenantManager.EnableTenantAsync(user, item);
            if (!result.IsFailed)
            {
                var message = _localizer["The team {0} is pending activation"];
                StatusMessage = message.Value.Replace("{0}", Input.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDisableAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var item = await _tenantManager.GetTenantAsync(user, Input.Id);
            if (item is null)
                return NotFound($"Unable to load team with ID '{Input.Id}'.");

            var result = await _tenantManager.DisableTenantAsync(user, item);
            if (!result.IsFailed)
            {
                var message = _localizer["The team {0} has been disabled"];
                StatusMessage = message.Value.Replace("{0}", Input.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty; 

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDefaultsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var item = await _tenantManager.GetTenantAsync(user, Input.Id);
            if (item is null)
                return NotFound($"Unable to load team with ID '{Input.Id}'.");

            var result = await _tenantManager.CreateDefaultsForTenantAsync(user, item);
            if (!result.IsFailed)
            {
                var message = _localizer["Defaults for team {0} are being created"];
                StatusMessage = message.Value.Replace("{0}", Input.Id);
            }
            else
                StatusMessage = result.Errors?.FirstOrDefault()?.Message ?? string.Empty;

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId, string roleId, string tenantId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var item = await _tenantManager.GetTenantAsync(user, tenantId);
            if (item is null)
                return NotFound($"Unable to load team with ID '{tenantId}'.");

            var result = await _tenantManager.RemoveUserFromTenantAsync(user, userId, roleId, tenantId);
            StatusMessage = string.Empty;
            foreach (var reason in result.Reasons)
            {
                StatusMessage += !string.IsNullOrEmpty(StatusMessage) ? Environment.NewLine : string.Empty;
                StatusMessage += reason.Message;
            }

            return RedirectToPage("Edit", "", new { id = tenantId });
        }
    }
}
