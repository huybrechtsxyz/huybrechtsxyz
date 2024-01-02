using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityUserRole")]
public class ApplicationUserRole : IdentityUserRole<string>
{
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string TenantId { get; set; } = string.Empty;
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.Property<string>("UserId").HasColumnType("nvarchar(450)");
        builder.Property<string>("TenantId").HasMaxLength(24).HasColumnType("nvarchar(24)");
        builder.Property<string>("RoleId").HasColumnType("nvarchar(450)");
        builder.HasKey("UserId", "TenantId", "RoleId");
        builder.HasIndex("TenantId", "RoleId");
        builder.ToTable("IdentityUserRole");
        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.HasOne<ApplicationTenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.NoAction).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserRole");
    }
}