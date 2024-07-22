using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform measure unit. Hours, Workspaces, ...
/// </summary>
[MultiTenant]
[Table("PlatformMeasure")]
[DisplayName("Platform Measure")]
public record PlatformMeasureUnit
{
    [Key]
    [Required]
    [DisplayName("ID")]
    [Comment("PlatformMeasure PK")]
    public int Id { get; set;} = 0;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;
}