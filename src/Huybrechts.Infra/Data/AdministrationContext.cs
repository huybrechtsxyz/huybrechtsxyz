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
        base.OnModelCreating(builder);

		builder.Entity<ApplicationRole>().ToTable("IdentityRole");
        builder.Entity<ApplicationRoleClaim>().ToTable("IdentityRoleClaim");
        builder.Entity<ApplicationUser>().ToTable("IdentityUser");
        builder.Entity<ApplicationUserClaim>().ToTable("IdentityUserClaim");
        builder.Entity<ApplicationUserLogin>().ToTable("IdentityUserLogin");
        builder.Entity<ApplicationUserRole>().ToTable("IdentityUserRole");
        builder.Entity<ApplicationUserToken>().ToTable("IdentityUserToken");
    }

    public DbSet<ApplicationTenant> Tenants { get; set; }

    //public DbSet<ApplicationTenantRole> ApplicationRoles { get; set; }

    //public DbSet<ApplicationTenantUser> ApplicationTenantUsers { get; set; }

    //public DbSet<ApplicationTenantUserRole> ApplicationTenantRoles { get; set; }
}

