using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a specific service offered by a platform, such as 'APIm - Standard' in Azure.
/// A platform can have multiple services, each potentially linked to different regions and products.
/// </summary>
[MultiTenant]
[Table("PlatformService")]
[Index(nameof(PlatformInfoId), nameof(Name), IsUnique = true)]
public record PlatformService : Entity, IEntity
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
    /// Foreign key to the PlatformRegion entity.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformRegion entity.")]
    public Ulid? PlatformRegionId { get; set; }

    /// <summary>
    /// Foreign key to the PlatformProduct entity.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformProduct entity.")]
    public Ulid? PlatformProductId { get; set; }

    /// <summary>
    /// The name of the service.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("The name of the service offered by the platform.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The label of the service, often used in user interfaces.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("A label representing the service, often used for display purposes.")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the service.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing details about the service.")]
    public string? Description { get; set; }

    /// <summary>
    /// The cost driver for the service, explaining what factors influence the cost.
    /// </summary>
    [MaxLength(256)]
    [Comment("The cost driver or factor that influences the pricing of the service.")]
    public string? CostDriver { get; set; }

    /// <summary>
    /// Describes the parameters on which the service pricing is based.
    /// </summary>
    [MaxLength(128)]
    [Comment("Parameters or metrics on which the cost of the service is based.")]
    public string? CostBasedOn { get; set; }

    /// <summary>
    /// Any limitations or constraints associated with the service.
    /// </summary>
    [MaxLength(512)]
    [Comment("Limitations or constraints related to the service.")]
    public string? Limitations { get; set; }

    /// <summary>
    /// URL for more information about the service.
    /// </summary>
    [MaxLength(512)]
    [Comment("URL linking to additional information about the service.")]
    public string? AboutURL { get; set; }

    /// <summary>
    /// URL for pricing details of the service.
    /// </summary>
    [MaxLength(512)]
    [Comment("URL providing pricing information for the service.")]
    public string? PricingURL { get; set; }

    /// <summary>
    /// Original identifier for the service, often used for external references.
    /// </summary>
    [MaxLength(64)]
    [Comment("Original identifier used to reference the service.")]
    public string? ServiceId { get; set; }

    /// <summary>
    /// Original name of the service, used for external identification.
    /// </summary>
    [MaxLength(64)]
    [Comment("Original name of the service used for external identification.")]
    public string? ServiceName { get; set; }

    /// <summary>
    /// Category or family of the service
    /// </summary>
    [MaxLength(128)]
    [Comment("Service family or category")]
    public string? ServiceFamily { get; set; }

    /// <summary>
    /// The size or pricing tier of the service.
    /// </summary>
    [MaxLength(128)]
    [Comment("Size or pricing tier associated with the service.")]
    public string? Size { get; set; }

    /// <summary>
    /// Additional remarks or notes about the service.
    /// </summary>
    [Comment("Additional remarks or notes regarding the service.")]
    public string? Remark { get; set; }
}
