using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform (Azure, On-Premise)
/// </summary>
[Table("PlatformMeasureDefault")]
[DisplayName("Platform Measure Default")]
public record PlatformMeasureDefault
{
    [Key]
    [Required]
    [DisplayName("Default ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [DisplayName("Provider ID")]
    [Comment("PlatformProvider FK")]
    public int PlatformProviderId { get; set;} = 0;

    [Required]
    [DisplayName("Measure ID")]
    [Comment("PlatformMeasureUnit FK")]
    public int PlatformMeasureUnitId { get; set;} = 0;

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

    /// <summary>
    /// Navigate to Services
    /// </summary>
    public virtual ICollection<PlatformService>? Services{ get; set; } = [];
}