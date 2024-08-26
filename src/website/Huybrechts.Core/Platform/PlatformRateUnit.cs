using Finbuckle.MultiTenant;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a unit of measurement for a rate within a platform's product offering. 
/// This class is used to convert platform-specific units into standard project metrics 
/// that are understandable to users.
/// </summary>
[MultiTenant]
[Table("PlatformRateUnit")]
[Comment("Table representing a unit of measurement for a rate within a platform's product offering, translating platform-specific units into standard project metrics.")]
public record PlatformRateUnit : Entity, IEntity
{
    /// <summary>
    /// Foreign key referencing the PlatformInfo table.
    /// This links the rate to a specific platform, ensuring that rates are associated with the correct environment.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformInfo entity.")]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Foreign key referencing the PlatformProduct table.
    /// This links the rate to a specific product offered on the platform, allowing for accurate pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformProduct entity.")]
    public Ulid PlatformProductId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Foreign key referencing the PlatformRate table.
    /// This links the rate to a specific product offered on the platform, allowing for accurate pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformProduct entity.")]
    public Ulid PlatformRateId { get; set; } = Ulid.Empty;

    // <summary>
    /// Navigation property for PlatformRate.
    /// </summary>
    [ForeignKey(nameof(PlatformRateId))]
    public PlatformRate PlatformRate { get; set; } = null!;

    /// <summary>
    /// Foreign key linking to the SetupUnit entity.
    /// </summary>
    [Required]
    [Comment("Foreign key linking to the SetupUnit entity.")]
    public Ulid SetupUnitId { get; set; }

    /// <summary>
    /// Navigation property for SetupUnit.
    /// </summary>
    public SetupUnit SetupUnit { get; set; } = null!;

    /// <summary>
    /// The conversion factor for the unit rate, used to translate platform units to standard units.
    /// </summary>
    [Required]
    [Precision(12, 6)]
    [DisplayName("Unit Factor")]
    [Comment("Conversion factor for the unit rate, translating platform units to standard units.")]
    public decimal UnitFactor { get; set; } = 0;

    /// <summary>
    /// The default rate for the unit, representing a base measurement standard.
    /// </summary>
    [Required]
    [Precision(12, 4)]
    [DisplayName("Default Unit")]
    [Comment("Default rate for the unit, representing a base measurement standard.")]
    public decimal DefaultValue { get; set; } = 0;

    /// <summary>
    /// Description of the measuring unit to provide additional context for users.
    /// </summary>
    [MaxLength(200)]
    [DisplayName("Description")]
    [Comment("Description of the measuring unit, providing additional context for users.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
