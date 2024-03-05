using Huybrechts.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUser")]
[EntityTypeConfiguration(typeof(ApplicationUserConfiguration))]
public class ApplicationUser : IdentityUser
{
    [StringLength(128)]
    public string? GivenName { get; set; }

    [StringLength(128)]
    public string? Surname { get; set; }

    public byte[]? ProfilePicture { get; set; }

    [NotMapped]
    public string Fullname => GivenName + (GivenName?.Length > 0 && Surname?.Length > 0 ? " " : "") + Surname;

    public virtual ICollection<ApplicationUserTenant> Tenants { get; set; } = new List<ApplicationUserTenant>();
}

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property<string>("Id").HasColumnType("nvarchar(450)");
        builder.Property<int>("AccessFailedCount").HasColumnType("int");
        builder.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
        builder.Property<string>("Email").HasMaxLength(256).HasColumnType("nvarchar(256)");
        switch (ApplicationContext.GlobalDatabaseProvider) {
		    case DatabaseProviderType.PostgreSQL:
			{ builder.Property<bool>("EmailConfirmed").HasColumnType("boolean"); break; }
		    default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
			{ builder.Property<bool>("EmailConfirmed").HasColumnType("bit"); break; }
		}
		builder.Property<string>("GivenName").HasMaxLength(128).HasColumnType("nvarchar(128)");
        switch (ApplicationContext.GlobalDatabaseProvider) {
			case DatabaseProviderType.PostgreSQL:
			{ builder.Property<bool>("LockoutEnabled").HasColumnType("boolean"); break; }
		    default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
			{ builder.Property<bool>("LockoutEnabled").HasColumnType("bit"); break; }
		}
        builder.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
        builder.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
        builder.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
		switch (ApplicationContext.GlobalDatabaseProvider) {
			case DatabaseProviderType.PostgreSQL:
				{ builder.Property<bool>("PhoneNumberConfirmed").HasColumnType("boolean"); break; }
			default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
				{ builder.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit"); break; }
		}
        builder.Property<byte[]>("ProfilePicture").HasColumnType("varbinary(max)");
        builder.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
        builder.Property<string>("Surname").HasMaxLength(128).HasColumnType("nvarchar(128)");
		switch (ApplicationContext.GlobalDatabaseProvider) {
			case DatabaseProviderType.PostgreSQL:
				{ builder.Property<bool>("TwoFactorEnabled").HasColumnType("boolean"); break; }
			default: //DatabaseProviderType.SqlServer || DatabaseProviderType.SqlLite
				{ builder.Property<bool>("TwoFactorEnabled").HasColumnType("bit"); break; }
		}
        builder.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.HasKey("Id");
        builder.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
        builder.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex").HasFilter("[NormalizedUserName] IS NOT NULL");
        builder.ToTable("IdentityUser");
    }
}