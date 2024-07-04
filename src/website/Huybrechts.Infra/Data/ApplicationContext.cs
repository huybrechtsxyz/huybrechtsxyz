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
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            // Each User can have many UserClaims
            b.HasMany(e => e.Claims)
                .WithOne(u => u.User)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Each User can have many UserLogins
            b.HasMany(e => e.Logins)
                .WithOne(u => u.User)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Each User can have many UserTokens
            b.HasMany(e => e.Tokens)
                .WithOne(u => u.User)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.ToTable(nameof(ApplicationUser));
        });

        builder.Entity<ApplicationRole>(b =>
        {
            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Each Role can have many associated RoleClaims
            b.HasMany(e => e.RoleClaims)
                .WithOne(e => e.Role)
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.ToTable(nameof(ApplicationRole));
        });

        builder.Entity<ApplicationTenant>().ToTable(nameof(ApplicationTenant));
        builder.Entity<ApplicationRoleClaim>().ToTable(nameof(ApplicationRoleClaim));
        builder.Entity<ApplicationUserClaim>().ToTable(nameof(ApplicationUserClaim));
        builder.Entity<ApplicationUserLogin>().ToTable(nameof(ApplicationUserLogin));
        builder.Entity<ApplicationUserRole>().ToTable(nameof(ApplicationUserRole));
        builder.Entity<ApplicationUserToken>().ToTable(nameof(ApplicationUserToken));
    }

    public DbSet<ApplicationTenant> ApplicationTenants { get; set; }
}
