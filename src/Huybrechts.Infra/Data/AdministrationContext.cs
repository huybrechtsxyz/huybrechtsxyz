using Huybrechts.Infra.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Data;

public class AdministrationContext
	: IdentityDbContext<
		ApplicationUser, 
		ApplicationRole, string, 
		ApplicationUserClaim, 
		ApplicationUserRole, 
		ApplicationUserLogin, 
		ApplicationRoleClaim, 
		ApplicationUserToken>
{
    public AdministrationContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ApplicationRoleConfiguration());
        builder.ApplyConfiguration(new ApplicationRoleClaimConfiguration());
        builder.ApplyConfiguration(new ApplicationTenantConfiguration());
        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new ApplicationUserClaimConfiguration());
        builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
        builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
        builder.ApplyConfiguration(new ApplicationUserTenantConfiguration());
        builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());
    }

    public DbSet<ApplicationTenant> Tenants { get; set; }

    public DbSet<ApplicationUserTenant> UserTenants { get; set; }
}

