using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityRoleClaim")]
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}

public class ApplicationRoleClaimConfigurationMsSql : ApplicationRoleClaimConfiguration
{
    public new void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
		base.Configure(builder);
    }
}

public class ApplicationRoleClaimConfigurationPgSql : ApplicationRoleClaimConfiguration
{
	public new void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
		base.Configure(builder);
	}
}

public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
	public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
	{
		builder.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
		builder.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
		builder.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
		builder.HasKey("Id");
		builder.HasIndex("RoleId");
		builder.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired();
		builder.ToTable("IdentityRoleClaim");
	}
}