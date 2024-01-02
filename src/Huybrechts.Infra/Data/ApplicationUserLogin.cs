using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityUserLogin")]
public class ApplicationUserLogin : IdentityUserLogin<string>
{
}

public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        builder.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
        builder.Property<string>("ProviderKey").HasColumnType("nvarchar(450)");
        builder.Property<string>("ProviderDisplayName").HasColumnType("nvarchar(max)");
        builder.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
        builder.HasKey("LoginProvider", "ProviderKey");
        builder.HasIndex("UserId");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserLogin");
    }
}
