using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserClaim")]
public class ApplicationUserClaim : IdentityUserClaim<string>
{
}

public class ApplicationUserClaimConfigurationMsSql : ApplicationUserClaimConfiguration
{
	public new void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
		base.Configure(builder);
	}
}

public class ApplicationUserClaimConfigurationPgSql : ApplicationUserClaimConfiguration
{
	public new void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
		base.Configure(builder);
	}
}

public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        builder.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
        builder.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
        builder.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
        builder.HasKey("Id");
        builder.HasIndex("UserId");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserClaim");
    }
}
