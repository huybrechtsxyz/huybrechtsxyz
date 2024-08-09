using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The products of the platform
/// Azure -> APIm - Standard
/// One Platform has multiple products
/// </summary>
[MultiTenant]
[Table("PlatformProduct")]
public record PlatformResource : Entity, IEntity
{
    /// <summary>
    /// PlatformInfo FK
    /// </summary>
    [Required]
    [Comment("PlatformInfo FK")]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Navigate to PlatformInfo
    /// </summary>
    public virtual PlatformInfo PlatformInfo { get; set; } = new();

    /// <summary>
    /// PlatformRegion FK
    /// </summary>
    [Required]
    [Comment("PlatformRegion FK")]
    public Ulid? PlatformRegionId { get; set; }

    /// <summary>
    /// PlatformService FK
    /// </summary>
    [Required]
    [Comment("PlatformService FK")]
    public Ulid? PlatformServiceId { get; set;}

    /// <summary>
    /// Name of the product
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the product")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Label of the product
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Label of the product")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    [MaxLength(256)]
    [Comment("Description of the product")]
    public string? Description { get; set; }

    /// <summary>
    /// Cost driver of the product
    /// </summary>
    [MaxLength(256)]
    [Comment("Cost driver")]
    public string? CostDriver { get; set; }

    /// <summary>
    /// The cost is based on ? for the product
    /// </summary>
    [MaxLength(128)]
    [Comment("Cost is based on what parameters for the product")]
    public string? CostBasedOn { get; set; }

    /// <summary>
    /// Limitations of the product
    /// </summary>
    [MaxLength(512)]
    [Comment("Limitations of the product")]
    public string? Limitations { get; set; }

    /// <summary>
    /// About Link
    /// </summary>
    [MaxLength(512)]
    [Comment("About Link")]
    public string? AboutURL { get; set; }

    /// <summary>
    /// Pricing url
    /// </summary>
    [MaxLength(512)]
    [Comment("Pricing url")]
    public string? PricingURL { get; set; }

    /// <summary>
    /// Original id of the product
    /// </summary>
    [MaxLength(64)]
    [Comment("Original id of the product")]
    public string? ProductId { get; set; }

    /// <summary>
    /// Original name of the product
    /// </summary>
    [MaxLength(64)]
    [Comment("Original name of the product")]
    public string? ProductName { get; set; }

    /// <summary>
    /// Size/pricing tier of the product
    /// </summary>
    [MaxLength(128)]
    [Comment("Size/pricing tier of the product")]
    public string? Size { get; set; }

    /// <summary>
    /// Size/pricing tier of the product
    /// </summary>
    [Comment("Remarks about the product")]
    public string? Remark { get; set; }
}