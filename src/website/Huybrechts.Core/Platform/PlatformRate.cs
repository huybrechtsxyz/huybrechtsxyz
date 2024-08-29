using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents the pricing rate of a service on a platform, including details such as the currency, price, and validity period.
/// This entity is used to track different rates for services across various regions, currencies, and time periods.
/// </summary>
[MultiTenant]
[Table("PlatformRate")]
[Comment("Represents the pricing rate of a service on a platform, including details such as the currency, price, and validity period.")]
[Index(nameof(SearchIndex))]
public record PlatformRate : Entity, IEntity
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
    /// Navigation property for the associated PlatformProduct.
    /// Provides access to details about the product this rate applies to.
    /// </summary>
    public PlatformProduct PlatformProduct { get; set; } = new();

    /// <summary>
    /// Foreign key referencing the PlatformRegion entity.
    /// Associates the rate with a specific region, reflecting geographical differences in pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformRegion entity.")]
    public Ulid PlatformRegionId { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformService entity.
    /// Links the rate to a specific service category, ensuring accurate pricing across products.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformService entity.")]
    public Ulid PlatformServiceId { get; set; }

    /// <summary>
    /// The name of the service.
    /// Provides a human-readable identifier for the service associated with this rate.
    /// </summary>
    [MaxLength(128)]
    [Comment("The name of the service.")]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The category or family of the service.
    /// Groups related services together under a common category.
    /// </summary>
    [MaxLength(128)]
    [Comment("Service family or category.")]
    public string ServiceFamily { get; set; } = string.Empty;

    /// <summary>
    /// The name of the product associated with this rate.
    /// Typically refers to the overarching product category the service belongs to, ensuring clarity in billing.
    /// </summary>
    [MaxLength(128)]
    [Comment("Product name.")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// The SKU (Stock Keeping Unit) name associated with this rate.
    /// Identifies the specific version or configuration of the product.
    /// </summary>
    [MaxLength(128)]
    [Comment("SKU name.")]
    public string SkuName { get; set; } = string.Empty;

    /// <summary>
    /// The meter name associated with this rate.
    /// Typically refers to the specific resource or unit being measured for billing purposes.
    /// </summary>
    [MaxLength(128)]
    [Comment("Meter name.")]
    public string MeterName { get; set; } = string.Empty;

    /// <summary>
    /// The type of rate (e.g., consumption, subscription).
    /// Categorizes the rate for different billing models, allowing for flexibility in pricing.
    /// </summary>
    [MaxLength(128)]
    [Comment("Rate type.")]
    public string RateType { get; set; } = string.Empty;

    /// <summary>
    /// The currency code in which the rate is expressed.
    /// Follows standard ISO currency codes (e.g., USD, EUR) to ensure clarity in multi-currency environments.
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Comment("Currency code.")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// The date and time from which this rate is valid.
    /// Allows for tracking of rate changes over time, ensuring historical accuracy.
    /// </summary>
    [Comment("Rate is valid from.")]
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// The retail price of the service in the specified currency.
    /// This is the price that customers will typically see.
    /// </summary>
    [Precision(12, 6)]
    [Comment("Retail price.")]
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// The unit price of the service in the specified currency.
    /// This is the price per unit of the service, providing granularity in pricing.
    /// </summary>
    [Precision(12, 6)]
    [Comment("Unit price.")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// The minimum number of units for which this rate applies.
    /// Useful for tiered pricing models where different rates apply to different usage levels.
    /// </summary>
    [Precision(12, 6)]
    [Comment("Tier minimum units.")]
    public decimal MinimumUnits { get; set; } = 0;

    /// <summary>
    /// The unit of measure for the rate (e.g., per hour, per GB).
    /// Describes what the rate applies to, ensuring clarity in billing metrics.
    /// </summary>
    [MaxLength(64)]
    [Comment("Unit of measure.")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is the primary rate for the region.
    /// Useful in cases where multiple rates are defined for the same region, but one is considered primary.
    /// </summary>
    [Comment("Indicates whether this is the primary rate for the region.")]
    public bool IsPrimaryRegion { get; set; } = false;

    /// <summary>
    /// Additional remarks or comments about the rate.
    /// Provides a space for any additional information or notes related to the rate.
    /// </summary>
    [DisplayName("Remark")]
    [Comment("Additional remarks or comments about the rate.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Navigation property to the linked rate units
    /// </summary>
    public virtual ICollection<PlatformRateUnit> RateUnits { get; set; } = [];
}