using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents the services or types of resources available on a platform.
/// For example, in Azure: API Management.
/// </summary>
[MultiTenant]
[Table("PlatformService")]
[Comment("Services offered by the platform, such as compute, storage, or networking resources.")]
[Index(nameof(TenantId), nameof(PlatformInfoId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
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
    /// The name of the service, such as a specific service or resource type.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("The name of the service or service offered by the platform.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The label of the service, typically used to identify it in the user interface.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("A label representing the service, often used in the user interface.")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The category or family to which the service belongs, helping to group similar services.
    /// </summary>
    [MaxLength(128)]
    [Comment("The category or family of the service, helping to classify it among similar offerings.")]
    public string? Category { get; set; }

    /// <summary>
    /// A brief description of the service.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing additional details about the service.")]
    public string? Description { get; set; }

    /// <summary>
    /// Any additional remarks about the service.
    /// </summary>
    [Comment("Additional remarks or notes regarding the service.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
