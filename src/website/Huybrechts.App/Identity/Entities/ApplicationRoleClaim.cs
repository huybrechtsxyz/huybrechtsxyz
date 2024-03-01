using Huybrechts.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityRoleClaim")]
[EntityTypeConfiguration(typeof(ApplicationRoleClaimConfiguration))]
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}

public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
	public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		switch (DatabaseContext.GlobalDatabaseProvider) {
			case DatabaseProviderType.PostgreSQL:
				{ NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id")); break; }
			default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
				{ SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id")); break; }
		}
		builder.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
		builder.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
		builder.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
		builder.HasKey("Id");
		builder.HasIndex("RoleId");
		builder.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired();
		builder.ToTable("IdentityRoleClaim");
	}
}