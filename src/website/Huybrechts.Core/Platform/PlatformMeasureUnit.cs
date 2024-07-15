using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform measure unit. Hours, Workspaces, ...
/// </summary>
[Table("PlatformMeasure")]
[DisplayName("Platform Measure")]
public record PlatformMeasureUnit
{
    [Key]
    [Required]
    [DisplayName("Measure ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;
}