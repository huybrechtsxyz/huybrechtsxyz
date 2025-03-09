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
[Index(nameof(TenantId), nameof(PlatformInfoId), nameof(SearchIndex))]
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
    [Comment("Foreign key linking to the SetupUnit entity.")]
    public Ulid? SetupUnitId { get; set; }

    /// <summary>
    /// Navigation property for SetupUnit.
    /// </summary>
    [ForeignKey(nameof(SetupUnitId))]
    public SetupUnit? SetupUnit { get; set; }

    /// <summary>
    /// Gets or sets the sequence order of this unit.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which units should be arranged or processed.
    /// </remarks>
    [Comment("Gets or sets the sequence order of this unit.")]
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// The name of the service.
    /// Provides a human-readable identifier for the service associated with this rate.
    /// </summary>
    [MaxLength(128)]
    [Comment("The name of the service.")]
    public string? ServiceName { get; set; }

    /// <summary>
    /// The name of the product associated with this rate.
    /// Typically refers to the overarching product category the service belongs to, ensuring clarity in billing.
    /// </summary>
    [MaxLength(128)]
    [Comment("Product name.")]
    public string? ProductName { get; set; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) name associated with this rate.
    /// Identifies the specific version or configuration of the product.
    /// </summary>
    [MaxLength(128)]
    [Comment("SKU name.")]
    public string? SkuName { get; set; }

    /// <summary>
    /// The meter name associated with this rate.
    /// Typically refers to the specific resource or unit being measured for billing purposes.
    /// </summary>
    [MaxLength(128)]
    [Comment("Meter name.")]
    public string? MeterName { get; set; }

    /// <summary>
    /// The unit of measure for the rate (e.g., per hour, per GB).
    /// Describes what the rate applies to, ensuring clarity in billing metrics.
    /// </summary>
    [MaxLength(64)]
    [Comment("Unit of measure.")]
    public string? UnitOfMeasure { get; set; }

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
    /// Is this a default for the Platform Rate Unit
    /// </summary>
    [Comment("Is this a default for the Platform Rate Unit")]
    public bool IsDefaultPlatformRateUnit { get; set; } = false;

    /// <summary>
    /// Is this a default for the Project Component Unit
    /// </summary>
    [Comment("Is this a default for the Project Component Unit")]
    public bool IsDefaultProjectComponentUnit { get; set; } = false;

    /// <summary>
    /// Gets or sets the name of the variable used in calculations.
    /// </summary>
    /// <remarks>
    /// This represents the variable name used in formulas for calculating the cost.
    /// </remarks>
    [MaxLength(128)]
    [Comment("The variable name used in the metrics calculations for this unit.")]
    public string? Variable { get; set; }

    /// <summary>
    /// Gets or sets the formula expression used to calculate the quantity value.
    /// </summary>
    /// <remarks>
    /// This formula will be applied to derive the cost or value of the input metrics.
    /// </remarks>
    [MaxLength(256)]
    [Comment("The formula used to calculate the value of the quantity for this unit.")]
    public string? Expression { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
