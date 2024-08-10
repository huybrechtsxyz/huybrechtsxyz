using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The services of the platform
/// Azure -> APIm - Standard
/// One Platform has multiple services
/// </summary>
[MultiTenant]
[Table("PlatformService")]
public record PlatformService : Entity, IEntity
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
    /// PlatformProduct FK
    /// </summary>
    [Required]
    [Comment("PlatformProduct FK")]
    public Ulid? PlatformProductId { get; set;}

    /// <summary>
    /// Name of the service
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the service")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Label of the service
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Label of the service")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Description of the service
    /// </summary>
    [MaxLength(256)]
    [Comment("Description of the service")]
    public string? Description { get; set; }

    /// <summary>
    /// Cost driver of the service
    /// </summary>
    [MaxLength(256)]
    [Comment("Cost driver")]
    public string? CostDriver { get; set; }

    /// <summary>
    /// The cost is based on ? for the service
    /// </summary>
    [MaxLength(128)]
    [Comment("Cost is based on what parameters for the service")]
    public string? CostBasedOn { get; set; }

    /// <summary>
    /// Limitations of the service
    /// </summary>
    [MaxLength(512)]
    [Comment("Limitations of the service")]
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
    /// Original id of the service
    /// </summary>
    [MaxLength(64)]
    [Comment("Original id of the service")]
    public string? ServiceId { get; set; }

    /// <summary>
    /// Original name of the service
    /// </summary>
    [MaxLength(64)]
    [Comment("Original name of the service")]
    public string? ServiceName { get; set; }

    /// <summary>
    /// Size/pricing tier of the service
    /// </summary>
    [MaxLength(128)]
    [Comment("Size/pricing tier of the service")]
    public string? Size { get; set; }

    /// <summary>
    /// Size/pricing tier of the service
    /// </summary>
    [Comment("Remarks about the service")]
    public string? Remark { get; set; }
}