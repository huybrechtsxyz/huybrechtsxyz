using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityTenant")]
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

    [StringLength(24)]
    public string? DatabaseProvider { get; set; }

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
        builder.Property<byte[]>("ConcurrencyStamp").IsConcurrencyToken().ValueGeneratedOnAddOrUpdate().HasColumnType("rowversion");
        builder.Property<string>("ConnectionString").HasMaxLength(512).HasColumnType("nvarchar(512)");
        builder.Property<string>("DatabaseProvider").HasMaxLength(24).HasColumnType("nvarchar(24)");
        builder.Property<string>("Description").HasMaxLength(512).HasColumnType("nvarchar(512)");
        builder.Property<string>("Name").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<byte[]>("ProfilePicture").HasColumnType("varbinary(max)");
        builder.Property<string>("Remark").HasColumnType("nvarchar(max)");
        builder.Property(p => p.State).HasConversion(v => v.ToString(), v => (ApplicationTenantState)Enum.Parse(typeof(ApplicationTenantState), v));
        builder.HasKey("Id");
        builder.ToTable("IdentityTenant");
    }
}