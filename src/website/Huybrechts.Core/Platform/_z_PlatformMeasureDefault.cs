using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform (Azure, On-Premise)
/// </summary>
[MultiTenant]
[Table("PlatformMeasureDefault")]
[DisplayName("Platform Measure Default")]
public record PlatformMeasureDefault : Entity, IEntity
{
    [Required]
    [DisplayName("Measure ID")]
    [Comment("PlatformMeasureUnit FK")]
    public Ulid PlatformMeasureUnitId { get; set;} = Ulid.Empty;

    [Required]
    [MaxLength(32)]
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