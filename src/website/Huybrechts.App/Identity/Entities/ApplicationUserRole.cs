using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserRole")]
[EntityTypeConfiguration(typeof(ApplicationUserRoleConfiguration))]
public class ApplicationUserRole : IdentityUserRole<string>
{
	[NotMapped]
	public string TenantId => ApplicationRole.GetTenantId(RoleId);

	[NotMapped]
	public string Label => ApplicationRole.GetRoleLabel(RoleId);
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.Property<string>("UserId").HasColumnType("nvarchar(450)");
        builder.Property<string>("RoleId").HasColumnType("nvarchar(450)");
		builder.HasKey("UserId", "RoleId");
        builder.ToTable("IdentityUserRole");
        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserRole");
    }
}