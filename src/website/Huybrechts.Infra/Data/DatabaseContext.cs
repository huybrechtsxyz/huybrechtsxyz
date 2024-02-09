using Huybrechts.Core.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Huybrechts.Infra.Data;

public class DatabaseContext
	: IdentityDbContext<
		ApplicationUser,
		ApplicationRole, string, 
		ApplicationUserClaim, 
		ApplicationUserRole, 
		ApplicationUserLogin, 
		ApplicationRoleClaim, 
		ApplicationUserToken>
{
    public DatabaseProviderType DatabaseProviderType { get; set; } = DatabaseProviderType.SqlServer;

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
		switch (DatabaseProviderType)
		{
			case DatabaseProviderType.SqlLite:
				OnModelCreatingSqlServer(builder);
				break;
			case DatabaseProviderType.SqlServer:
				OnModelCreatingSqlServer(builder);
				break;
			case DatabaseProviderType.PostgreSQL:
				OnModelCreatingPostgres(builder);
				break;
			default: 
				throw new NotImplementedException("No or invalid database provider is registered");
		}
	}

	private void OnModelCreatingSqlServer(ModelBuilder builder)
	{
		builder.ApplyConfiguration(new ApplicationRoleConfiguration());
		builder.ApplyConfiguration(new ApplicationRoleClaimConfigurationMsSql());
		builder.ApplyConfiguration(new ApplicationTenantConfiguration());
		builder.ApplyConfiguration(new ApplicationUserConfiguration());
		builder.ApplyConfiguration(new ApplicationUserClaimConfigurationMsSql());
		builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
		builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
		builder.ApplyConfiguration(new ApplicationUserTenantConfigurationMsSql());
		builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());
	}

	private void OnModelCreatingPostgres(ModelBuilder builder)
	{
		builder.ApplyConfiguration(new ApplicationRoleConfiguration());
		builder.ApplyConfiguration(new ApplicationRoleClaimConfigurationPgSql());
		builder.ApplyConfiguration(new ApplicationTenantConfiguration());
		builder.ApplyConfiguration(new ApplicationUserConfiguration());
		builder.ApplyConfiguration(new ApplicationUserClaimConfigurationPgSql());
		builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
		builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
		builder.ApplyConfiguration(new ApplicationUserTenantConfigurationNpSql());
		builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());
	}

	public DbSet<ApplicationTenant> Tenants { get; set; }

    public DbSet<ApplicationUserTenant> UserTenants { get; set; }
}

