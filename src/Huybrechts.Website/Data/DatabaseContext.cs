using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Website.Data
{
	public class DatabaseContext(DbContextOptions<DatabaseContext> options) 
		: IdentityDbContext<ApplicationUser>(options)
	{
	}
}
