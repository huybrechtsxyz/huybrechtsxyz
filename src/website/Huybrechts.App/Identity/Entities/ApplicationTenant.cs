using Huybrechts.App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityTenant")]
[EntityTypeConfiguration(typeof(ApplicationTenantConfiguration))]
public record ApplicationTenant
{
    [Key]
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string Id { get; set; } = string.Empty;

    public ApplicationTenantState State { get; set; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public string? Remark { get; set; }

    public byte[]? Picture { get; set; }

    public DatabaseProviderType DatabaseProvider { get; set; }

    [StringLength(512)]
    public string? ConnectionString { get; set; }

    [Timestamp]
    public byte[]? ConcurrencyStamp { get; set; }

    public void UpdateFrom(ApplicationTenant entity)
    {
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Remark = entity.Remark;
    }
}

public class ApplicationTenantConfiguration : IEntityTypeConfiguration<ApplicationTenant>
{
    public void Configure(EntityTypeBuilder<ApplicationTenant> builder)
    {
        builder.Property<string>("Id").HasMaxLength(24).HasColumnType("nvarchar(24)");
        builder.Property<ApplicationTenantState>("State").HasConversion<string>();
		builder.Property<string>("Name").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
		builder.Property<string>("Description").HasMaxLength(512).HasColumnType("nvarchar(512)");
		builder.Property<string>("Remark").HasColumnType("nvarchar(max)");
		builder.Property<byte[]>("ProfilePicture").HasColumnType("varbinary(max)");
		builder.Property<DatabaseProviderType>("DatabaseProvider").HasConversion<string>();
		builder.Property<string>("ConnectionString").HasMaxLength(512).HasColumnType("nvarchar(512)");
        builder.Property<byte[]>("ConcurrencyStamp").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate().HasColumnType("rowversion");
        builder.HasKey("Id");
        builder.ToTable("IdentityTenant");
    }
}