using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The data center region of the platform
/// Azure -> WEU1 - West Europe
/// One Platform has multiple Locations
/// </summary>
[MultiTenant]
[Table("PlatformRegion")]
[Comment("Support regions of the Platform")]
public record PlatformRegion : Entity, IEntity
{
    /// <summary>
    /// FK to PlatformInfo
    /// </summary>
    [Required]
    [Comment("PlatformInfo FK")]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Navigate to PlatformInfo
    /// </summary>
    public virtual PlatformInfo PlatformInfo { get; set; } = new();

    /// <summary>
    /// Name of the region
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the region")]
    public string Name { get; set;} = string.Empty;

    /// <summary>
    /// Label of the region
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Label of the region")]
    public string Label { get; set;} = string.Empty;

    /// <summary>
    /// Description of the region
    /// </summary>
    [MaxLength(256)]
    [Comment("Description of the region")]
    public string? Description { get; set;}

    /// <summary>
    /// Remark about the region
    /// </summary>
    [Comment("Remark about the region")]
    public string? Remark { get; set; }
}