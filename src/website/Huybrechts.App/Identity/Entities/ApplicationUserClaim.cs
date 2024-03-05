using Huybrechts.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserClaim")]
[EntityTypeConfiguration(typeof(ApplicationUserClaimConfiguration))]
public class ApplicationUserClaim : IdentityUserClaim<string>
{
}

public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		switch (ApplicationContext.GlobalDatabaseProvider) {
			case DatabaseProviderType.PostgreSQL:
				{ NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id")); break; }
			default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
				{ SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id")); break; }
		}
		builder.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
        builder.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
        builder.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
        builder.HasKey("Id");
        builder.HasIndex("UserId");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserClaim");
    }
}
