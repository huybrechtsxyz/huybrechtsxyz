using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The Products available on the platform (type of resource)
/// Azure -> API Management
/// </summary>
[MultiTenant]
[Table("PlatformProduct")]
public record PlatformProduct : Entity, IEntity
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
    /// Name of the product
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the product")]
    public string Name { get; set;} = string.Empty;

    /// <summary>
    /// Label of the product
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Label of the product")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Category or family of the product
    /// </summary>
    [MaxLength(100)]
    [Comment("Product category")]
    public string? Category { get; set; }

    /// <summary>
    /// Description of the product
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