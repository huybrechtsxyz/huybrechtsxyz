using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The rate of a resource on a platform
/// </summary>
[MultiTenant]
[Table("PlatformRateUnit")]
[DisplayName("Rate unit")]
public record PlatformRateUnit
{
    [Key]
    [Required]
    [DisplayName("ID")]
    [Comment("PlatformRateUnit PK")]
    public Ulid Id { get; set; } = Ulid.Empty;

    [Required]
    [DisplayName("Provider ID")]
    [Comment("PlatformProvider FK")]
    public Ulid PlatformProviderId { get; set;} = Ulid.Empty;

    [Required]
    [DisplayName("Service ID")]
    [Comment("PlatformService FK")]
    public Ulid PlatformServiceId { get; set;} = Ulid.Empty;

    [Required]
    [DisplayName("Resource ID")]
    [Comment("PlatformResource FK")]
    public Ulid PlatformResourceId { get; set;} = Ulid.Empty;

    [Required]
    [DisplayName("Rate ID")]
    [Comment("PlatformRate FK")]
    public Ulid PlatformRateId { get; set; } = Ulid.Empty;

    [Required]
    [DisplayName("Measure Unit ID")]
    [Comment("PlatformMeasureUnit FK")]
    public Ulid PlatformMeasureUnitId { get; set; } = Ulid.Empty;

    [Required]
    [MaxLength(30)]
    [DisplayName("Unit of Measure")]
    [Comment("Unit of measure")]
    public string UnitOfMeasure { get; set; } = String.Empty;

    [Required]
    [Precision(12, 6)]
    [DisplayName("Unit Factor")]
    [Comment("Conversion factor")]
    public decimal UnitFactor { get; set; } = 0;

    [Required]
    [Precision(12, 4)]
    [DisplayName("Default unit")]
    [Comment("Default unit rate")]
    public decimal DefaultValue { get; set; } = 0;

    [MaxLength(200)]
    [DisplayName("Description")]
    [Comment("Measuring unit description")]
    public string Description { get; set; } = string.Empty;
}