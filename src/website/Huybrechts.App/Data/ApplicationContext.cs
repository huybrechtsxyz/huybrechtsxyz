using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Huybrechts.App.Data;

/// <summary>
/// The database context for the application.
/// Uses migration in the different infra projects
/// Package Manager:
///		Select appropriate infra package
///		Add-Migration xyz -Args "--provider PostgreSQL"
///		Add-Migration xyz -Args "--provider SqlLite"
///		Add-Migration xyz -args "--provider SqlServer"
///		Update-Database
/// </summary>
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
	public static DatabaseProviderType GlobalDatabaseProvider { get; set; }

    public DatabaseProviderType DatabaseProviderType { get; set; }

    public ApplicationContext(DbContextOptions options) : base(options) 
	{
		DatabaseProviderType = GlobalDatabaseProvider;
	}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
		base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
		builder.Entity<ApplicationRole>();
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

	public DbSet<ApplicationTenant> ApplicationTenants { get; set; }

    public DbSet<ApplicationUserTenant> ApplicationUserTenants { get; set; }
}

