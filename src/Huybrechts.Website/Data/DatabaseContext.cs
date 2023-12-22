using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Website.Data;

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
	//protected override void OnModelCreating(ModelBuilder builder)
	//{
        
    //}
}
