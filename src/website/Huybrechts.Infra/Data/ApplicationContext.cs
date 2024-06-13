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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ApplicationRole>().ToTable(nameof(ApplicationRole));
        builder.Entity<ApplicationRoleClaim>().ToTable(nameof(ApplicationRoleClaim));
        builder.Entity<ApplicationTenant>().ToTable(nameof(ApplicationTenant));
        builder.Entity<ApplicationUser>().ToTable(nameof(ApplicationUser));
        builder.Entity<ApplicationUserClaim>().ToTable(nameof(ApplicationUserClaim));
        builder.Entity<ApplicationUserLogin>().ToTable(nameof(ApplicationUserLogin));
        builder.Entity<ApplicationUserRole>().ToTable(nameof(ApplicationUserRole));
        builder.Entity<ApplicationUserToken>().ToTable(nameof(ApplicationUserToken));
    }

    public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
}
