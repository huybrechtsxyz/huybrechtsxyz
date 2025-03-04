using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Web.Pages.Account.Tenant
{
    public class AddUsersModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationTenantManager _tenantManager;
        private readonly IStringLocalizer<AddUsersModel> _localization;

        [BindProperty]
        public Invitation Input { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public ICollection<ApplicationRole> Roles { get; set; } = [];

        public class Invitation
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

        public AddUsersModel(ApplicationUserManager userManager, ApplicationTenantManager tenantManager, IStringLocalizer<AddUsersModel> localization)
        {
            _userManager = userManager;
            _tenantManager = tenantManager;
            _localization = localization;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound($"Unable to load team with ID '{id}'.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var tenant = await _tenantManager.GetTenantAsync(user, id);
            if (tenant is null)
                return NotFound($"Unable to load team with ID '{id}'.");

            Roles = await _tenantManager.GetTenantRolesAsync(user, id) ?? [];
            Input = new()
            {
                TenantId = tenant.Id,
                RoleId = string.Empty,
                Users = string.Empty
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
                var roles = await _tenantManager.GetTenantRolesAsync(user, Input.TenantId) ?? [];
                Roles = [.. roles.OrderBy(o => o.Label)];

                StatusMessage = _localization["Unexpected error when trying to add users to team."];
                return Page();
            }

            string tenantId = Input.TenantId;
            var newUsers = Input.Users?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray() ?? [];
            var result = await _tenantManager.AddUsersToTenantAsync(user, Input.TenantId, Input.RoleId, newUsers);
            StatusMessage = string.Empty;
            foreach(var item in result.Reasons)
            {
                StatusMessage += !string.IsNullOrEmpty(StatusMessage) ? Environment.NewLine : string.Empty;
                StatusMessage += item.Message;
            }
            return RedirectToPage("Edit", "", new { id = tenantId });
        }
    }
}
