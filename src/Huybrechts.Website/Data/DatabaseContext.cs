using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Huybrechts.Website.Data;

public enum DatabaseContextType
{
	None = 0,
	SqlServer = 1,
	PostgreSQL = 2
}

public class DatabaseContext(DbContextOptions<DatabaseContext> options)
	: IdentityDbContext<
		ApplicationUser, 
		ApplicationRole, string, 
		ApplicationUserClaim, 
		ApplicationUserRole, 
		ApplicationUserLogin, 
		ApplicationRoleClaim, 
		ApplicationUserToken>(options)
{
	protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

		builder.Entity<ApplicationRole>().ToTable("IdentityRole");
        builder.Entity<ApplicationRoleClaim>().ToTable("IdentityRoleClaim");
        builder.Entity<ApplicationUser>().ToTable("IdentityUser");
        builder.Entity<ApplicationUserClaim>().ToTable("IdentityUserClaim");
        builder.Entity<ApplicationUserLogin>().ToTable("IdentityUserLogin");
        builder.Entity<ApplicationUserRole>().ToTable("IdentityUserRole");
        builder.Entity<ApplicationUserToken>().ToTable("IdentityUserToken");
    }
}

