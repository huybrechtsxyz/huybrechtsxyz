using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a data center region within a platform. 
/// For example, in Azure: WEU1 - West Europe.
/// A single platform can have multiple regions.
/// </summary>
[MultiTenant]
[Table("PlatformRegion")]
[Comment("Regions supported by the platform, representing data center locations.")]
[Index(nameof(PlatformInfoId), nameof(Name), IsUnique = true)]
public record PlatformRegion : Entity, IEntity
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
    /// The name of the region, such as 'armRegionName'.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("The unique name identifier of the region.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The label of the region, typically the location name.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("A label representing the region, often the location name.")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the region.
    /// </summary>
    [MaxLength(256)]
    [Comment("A brief description providing additional details about the region.")]
    public string? Description { get; set; }

    /// <summary>
    /// Any additional remarks about the region.
    /// </summary>
    [Comment("Additional remarks or notes regarding the region.")]
    public string? Remark { get; set; }
}
