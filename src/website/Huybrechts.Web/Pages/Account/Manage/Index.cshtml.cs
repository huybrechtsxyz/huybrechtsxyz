﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Huybrechts.App.Application;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Huybrechts.Web.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IStringLocalizer<IndexModel> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        public byte[] Picture { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [DataType(DataType.Text)]
            [Display(Name = nameof(GivenName), Prompt = nameof(GivenName) + "_p", ResourceType = typeof(AccountViewModels))]
            public string GivenName { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [DataType(DataType.Text)]
            [Display(Name = nameof(Surname), Prompt = nameof(Surname) + "_p", ResourceType = typeof(AccountViewModels))]
            public string Surname { get; set; } = "";

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = nameof(PhoneNumber), Prompt = nameof(PhoneNumber) + "_p", ResourceType = typeof(AccountViewModels))]
            public string PhoneNumber { get; set; }

            public IFormFile ProfileFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                GivenName = user.GivenName,
                Surname = user.Surname
            };

            if (user.ProfilePicture is not null) { 
                Picture ??= new byte[user.ProfilePicture.Length];
                Array.Copy(user.ProfilePicture, Picture, user.ProfilePicture.Length);
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.GivenName.CompareTo(user.GivenName) != 0 || Input.Surname.CompareTo(user.Surname) != 0)
            {
                await _userManager.SetGivenSurNameAsync(user, Input.GivenName, Input.Surname);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = _localizer["Unexpected error when trying to set phone number."];
                    return RedirectToPage();
                }
            }

            IFormFile file = Request.Form.Files.FirstOrDefault();
            if (file is not null)
            {
                using var dataStream = new MemoryStream();
                await file.CopyToAsync(dataStream);
                user.ProfilePicture = new byte[dataStream.Length];
                user.ProfilePicture = dataStream.ToArray();
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["Your profile has been updated"];
            return RedirectToPage();
        }
    }
}
