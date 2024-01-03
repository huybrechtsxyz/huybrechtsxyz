using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Entities;

[Table("IdentityUserRole")]
public class ApplicationUserRole : IdentityUserRole<string>
{
    [NotMapped]
    public string TenantId
    {
        get
        {
            if (RoleId.IndexOf("#")==0)
                return string.Empty;
			return RoleId[0..(RoleId.IndexOf("#") - 1)];
		}
    }

    [NotMapped]
    public string Label
    { 
        get 
        {
            if (RoleId.IndexOf("#") == 0)
                return RoleId;
            return RoleId[(RoleId.IndexOf("#") + 1)..];
        } 
    }
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