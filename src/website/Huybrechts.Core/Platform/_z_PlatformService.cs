using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The services available on the platform (type of resource)
/// Azure -> API Management
/// </summary>
[MultiTenant]
[Table("PlatformService")]
[DisplayName("Service")]
public record PlatformService : Entity, IEntity
{
    [Required]
    [DisplayName("Platform Info ID")]
    [Comment("PlatformInfo FK")]
    public Ulid PlatformInfoId { get; set;} = Ulid.Empty;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;

    [MaxLength(256)]
    [DisplayName("Description")]
    [Comment("Description")]
    public string? Description { get; set;}

    [DisplayName("Remark")]
    [Comment("Remark")]
    public string? Remark { get; set; }

    [MaxLength(100)]
    [DisplayName("Category")]
    [Comment("Service Category")]
    public string? Category { get; set;}

    [DisplayName("Is Allowed?")]
    [Comment("Is the service allowed?")]
    public bool Allowed { get; set; } = false;

    /// <summary>
    /// Navigate to Resources
    /// </summary>
    public ICollection<PlatformResource> Resources { get; set; } = [];
}