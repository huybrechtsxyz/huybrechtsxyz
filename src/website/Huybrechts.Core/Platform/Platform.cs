using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform that can be used in project
/// </summary>
[MultiTenant]
[Table("Platform")]
[DisplayName("Platform")]
public record PlatformInfo : Entity, IEntity, ICopyableEntity
{
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

    /// <summary>
    /// Navigate to Services
    /// </summary>
    public virtual ICollection<PlatformService>? Services{ get; set; } = [];

    /// <summary>
    /// Copy parameters for update and creation
    /// </summary>
    /// <param name="info">Record to copy from</param>
    public void CopyFrom(PlatformInfo info)
    {
        Name = info.Name;
        Description = info.Description;
        Remark = info.Remark;
    }
}