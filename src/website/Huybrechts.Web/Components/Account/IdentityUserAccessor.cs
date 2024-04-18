using Huybrechts.App.Identity;
using Huybrechts.App.Identity.Entities;

namespace Huybrechts.Web.Components.Account
{
    internal sealed class IdentityUserAccessor(ApplicationUserManager userManager, IdentityRedirectManager redirectManager)
	{
		public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
		{
			var user = await userManager.GetUserAsync(context.User);

			if (user is null)
			{
				redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
			}

			return user;
		}
	}
}
