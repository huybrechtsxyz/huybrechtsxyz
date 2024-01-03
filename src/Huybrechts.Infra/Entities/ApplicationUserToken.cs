using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Entities;

[Table("IdentityUserToken")]
public class ApplicationUserToken : IdentityUserToken<string>
{
}

public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
{
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        builder.Property<string>("UserId").HasColumnType("nvarchar(450)");
        builder.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
        builder.Property<string>("Name").HasColumnType("nvarchar(450)");
        builder.Property<string>("Value").HasColumnType("nvarchar(max)");
        builder.HasKey("UserId", "LoginProvider", "Name");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserToken");
    }
}