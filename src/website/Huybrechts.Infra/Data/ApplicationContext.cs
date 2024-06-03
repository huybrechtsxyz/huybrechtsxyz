using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Data;

public class ApplicationContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole, string,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        ApplicationUserToken>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
    {

    }

    public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
}
