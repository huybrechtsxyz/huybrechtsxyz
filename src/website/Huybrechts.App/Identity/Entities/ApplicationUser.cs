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

        /// TODO
        builder.Property<bool>("EmailConfirmed").HasColumnType("bit");

        builder.Property<string>("GivenName").HasMaxLength(128).HasColumnType("nvarchar(128)");

        /// TODO
        builder.Property<bool>("LockoutEnabled").HasColumnType("bit");

        builder.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
        builder.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
        builder.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");

        /// TODO
        builder.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");

        builder.Property<byte[]>("ProfilePicture").HasColumnType("varbinary(max)");
        builder.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
        builder.Property<string>("Surname").HasMaxLength(128).HasColumnType("nvarchar(128)");

        /// TODO
        builder.Property<bool>("TwoFactorEnabled").HasColumnType("bit");


        builder.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.HasKey("Id");
        builder.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
        builder.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex").HasFilter("[NormalizedUserName] IS NOT NULL");
        builder.ToTable("IdentityUser");
    }
}