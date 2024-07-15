using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The rate of a resource on a platform
/// </summary>
[Table("PlatformRateUnit")]
[DisplayName("Rate unit")]
public record PlatformRateUnit
{
    [Key]
    [Required]
    [DisplayName("Rate Unit ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [DisplayName("Provider ID")]
    [Comment("PlatformProvider FK")]
    public int PlatformProviderId { get; set;} = 0;

    [Required]
    [DisplayName("Service ID")]
    [Comment("PlatformService FK")]
    public int PlatformServiceId { get; set;} = 0;

    [Required]
    [DisplayName("Resource ID")]
    [Comment("PlatformResource FK")]
    public int PlatformResourceId { get; set;} = 0;

    [Required]
    [DisplayName("Rate ID")]
    [Comment("PlatformRate FK")]
    public int PlatformRateId { get; set; } = 0;

    [Required]
    [DisplayName("Measure Unit ID")]
    [Comment("PlatformMeasureUnit FK")]
    public int PlatformMeasureUnitId { get; set; } = 0;
    
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