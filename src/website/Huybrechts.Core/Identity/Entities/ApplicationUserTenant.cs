using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Identity.Entities;

[Table("IdentityUserTenant")]
[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
public record ApplicationUserTenant
{
    [Key]
    public int Id { get; set; } = 0;

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string TenantId { get; set; } = string.Empty;

    public string? Remark { get; set; }

    [Timestamp]
    public string? ConcurrencyStamp { get; set; }
}

public class ApplicationUserTenantConfigurationMsSql : ApplicationUserTenantConfiguration
{
	public new void Configure(EntityTypeBuilder<ApplicationUserTenant> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
		SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
        base.Configure(builder);
	}
}
public class ApplicationUserTenantConfigurationNpSql : ApplicationUserTenantConfiguration
{
	public new void Configure(EntityTypeBuilder<ApplicationUserTenant> builder)
	{
		builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        NpgsqlPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
        base.Configure(builder);
	}
}

public class ApplicationUserTenantConfiguration : IEntityTypeConfiguration<ApplicationUserTenant>
{
    public void Configure(EntityTypeBuilder<ApplicationUserTenant> builder)
    {
        builder.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
        builder.Property<string>("TenantId").HasMaxLength(24).HasColumnType("nvarchar(24)");
        builder.Property<string>("Remark").HasColumnType("nvarchar(max)");
        builder.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
        builder.HasOne<ApplicationTenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityUserTenant");
    }
}