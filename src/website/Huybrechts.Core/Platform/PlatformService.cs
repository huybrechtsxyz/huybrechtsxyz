using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The services available on the platform (type of resource)
/// Azure -> API Management
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
    public Ulid PlatformInfoId { get; set;} = Ulid.Empty;

    /// <summary>
    /// Navigate to PlatformInfo
    /// </summary>
    public virtual PlatformInfo PlatformInfo { get; set; } = new();

    /// <summary>
    /// Name of the object
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;

    /// <summary>
    /// Label of the service
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Label of the service")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Category or family of the object
    /// </summary>
    [MaxLength(100)]
    [Comment("Service Category")]
    public string? Category { get; set; }

    /// <summary>
    /// Description of the object
    /// </summary>
    [MaxLength(256)]
    [Comment("Description")]
    public string? Description { get; set;}

    /// <summary>
    /// Remark
    /// </summary>
    [Comment("Remark")]
    public string? Remark { get; set; }
}