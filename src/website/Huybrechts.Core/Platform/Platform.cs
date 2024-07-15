using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform (Azure, On-Premise)
/// </summary>
[Table("Platform")]
[DisplayName("Platform")]
public record Platform
{
    [Key]
    [Required]
    [DisplayName("Platform ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [StringLength(24)]
    [DisplayName("Tenant ID")]
    [Comment("Tenant FK")]
    public string? ApplicationTenantId { get; set; }

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
}