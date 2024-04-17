using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.App.Data;

public class ApplicationContext
	: IdentityDbContext<
		ApplicationUser,
		ApplicationRole, string,
		ApplicationUserClaim,
		ApplicationUserRole, 
		ApplicationUserLogin, 
		ApplicationRoleClaim, 
		ApplicationUserToken>
{
	public ApplicationContext(DbContextOptions options) : base(options) 
	{
	}
}
