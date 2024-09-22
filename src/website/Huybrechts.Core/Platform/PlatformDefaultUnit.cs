using Finbuckle.MultiTenant;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a default unit of measure for a platform, linking to a setup unit and a specific platform.
/// </summary>
/// <remarks>
/// This entity is used to define default rate units that are associated with a particular platform.
/// Each unit of measure is linked to a setup unit from the `SetupUnit` table.
/// </remarks>
[MultiTenant]
[Table("PlatformDefaultUnit")]
[Comment("Represents a default unit of measure for a platform, linking to a setup unit and a specific platform.")]
[Index(nameof(TenantId), nameof(PlatformInfoId), nameof(UnitOfMeasure))]
[Index(nameof(TenantId), nameof(SearchIndex))]
public record PlatformDefaultUnit : Entity, IEntity
{
    /// <summary>
    /// Foreign key to the PlatformInfo entity.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformInfo.")]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Navigation property to the related PlatformInfo.
    /// </summary>
    public virtual PlatformInfo PlatformInfo { get; set; } = new();

    /// <summary>
    /// Foreign key linking to the SetupUnit entity.
    /// </summary>
    [Required]
    [Comment("Foreign key linking to the SetupUnit entity.")]
    public Ulid SetupUnitId { get; set; }

    /// <summary>
    /// Navigation property for SetupUnit.
    /// </summary>
    [ForeignKey(nameof(SetupUnitId))]
    public SetupUnit SetupUnit { get; set; } = null!;

    /// <summary>
    /// The unit of measure for the rate (e.g., per hour, per GB).
    /// Describes what the rate applies to, ensuring clarity in billing metrics.
    /// </summary>
    [MaxLength(64)]
    [Comment("Unit of measure.")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// The conversion factor for the unit rate, used to translate platform units to standard units.
    /// </summary>
    [Required]
    [Precision(18, 6)]
    [DisplayName("Unit Factor")]
    [Comment("Conversion factor for the unit rate, translating platform units to standard units.")]
    public decimal UnitFactor { get; set; } = 0;

    /// <summary>
    /// The default rate for the unit, representing a base measurement standard.
    /// </summary>
    [Required]
    [Precision(18, 4)]
    [DisplayName("Default Unit")]
    [Comment("Default rate for the unit, representing a base measurement standard.")]
    public decimal DefaultValue { get; set; } = 0;

    /// <summary>
    /// A brief description of the region.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing additional details about the region.")]
    public string? Description { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
