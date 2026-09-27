using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a specific product offered by a platform, such as 'APIm - Standard' in Azure.
/// A platform can have multiple products, each potentially linked to different regions and products.
/// </summary>
[MultiTenant]
[Table("PlatformProduct")]
[Comment("Represents a product offered on a specific platform, detailing attributes such as the product's name, description, and other relevant metadata.")]
[Index(nameof(TenantId), nameof(PlatformInfoId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
public record PlatformProduct : Entity, IEntity
{
    /// <summary>
    /// Foreign key to the PlatformInfo entity.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformInfo entity.")]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Navigation property to the related PlatformInfo.
    /// </summary>
    public virtual PlatformInfo PlatformInfo { get; set; } = new();

    /// <summary>
    /// The name of the product.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("The name of the product offered by the platform.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The label of the product, often used in user interfaces.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("A label representing the product, often used for display purposes.")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the product.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing details about the product.")]
    public string? Description { get; set; }

    /// <summary>
    /// Category of the product
    /// </summary>
    [MaxLength(128)]
    [Comment("Category of the product")]
    public string? Category { get; set; }

    /// <summary>
    /// The cost driver for the product, explaining what factors influence the cost.
    /// </summary>
    [MaxLength(256)]
    [Comment("The cost driver or factor that influences the pricing of the product.")]
    public string? CostDriver { get; set; }

    /// <summary>
    /// Describes the parameters on which the product pricing is based.
    /// </summary>
    [MaxLength(128)]
    [Comment("Parameters or metrics on which the cost of the product is based.")]
    public string? CostBasedOn { get; set; }

    /// <summary>
    /// Any limitations or constraints associated with the product.
    /// </summary>
    [MaxLength(512)]
    [Comment("Limitations or constraints related to the product.")]
    public string? Limitations { get; set; }

    /// <summary>
    /// URL for more information about the product.
    /// </summary>
    [MaxLength(512)]
    [Comment("URL linking to additional information about the product.")]
    public string? AboutURL { get; set; }

    /// <summary>
    /// URL for pricing details of the product.
    /// </summary>
    [MaxLength(512)]
    [Comment("URL providing pricing information for the product.")]
    public string? PricingURL { get; set; }

    /// <summary>
    /// The size or pricing tier of the product.
    /// </summary>
    [MaxLength(128)]
    [Comment("Size or pricing tier associated with the product.")]
    public string? PricingTier { get; set; }

    /// <summary>
    /// Additional remarks or notes about the product.
    /// </summary>
    [Comment("Additional remarks or notes regarding the product.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
