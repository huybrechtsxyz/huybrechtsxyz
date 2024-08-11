using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents the products or types of resources available on a platform.
/// For example, in Azure: API Management.
/// </summary>
[MultiTenant]
[Table("PlatformProduct")]
[Comment("Products or services offered by the platform, such as compute, storage, or networking resources.")]
[Index(nameof(PlatformInfoId), nameof(Name), IsUnique = true)]
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
    /// The name of the product, such as a specific service or resource type.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("The name of the product or service offered by the platform.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The label of the product, typically used to identify it in the user interface.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("A label representing the product, often used in the user interface.")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The category or family to which the product belongs, helping to group similar products.
    /// </summary>
    [MaxLength(100)]
    [Comment("The category or family of the product, helping to classify it among similar offerings.")]
    public string? Category { get; set; }

    /// <summary>
    /// A brief description of the product.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing additional details about the product.")]
    public string? Description { get; set; }

    /// <summary>
    /// Any additional remarks about the product.
    /// </summary>
    [Comment("Additional remarks or notes regarding the product.")]
    public string? Remark { get; set; }
}
