// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace Huybrechts.Web.Pages.Account.Manage
{
    public class UserRolesModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger<UserRolesModel> _logger;

        public ApplicationRole[] SystemRoles { get; set; } = [];

        public ApplicationRole[] TenantRoles { get; set; } = [];

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public UserRolesModel(
            ApplicationUserManager userManager,
            ILogger<UserRolesModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var roles = await _userManager.GetApplicationRolesAsync(user);
            SystemRoles = [.. roles.Where(q => q.TenantId.IsNullOrEmpty()).OrderBy(o => o.Label)];
            TenantRoles = [.. roles.Where(q => !q.TenantId.IsNullOrEmpty()).OrderBy(o => o.TenantId).ThenBy(o => o.Label)];

            return Page();
        }
    }
}
