using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The data center location of the platform
/// Azure -> WEU1 - West Europe
/// One Platform has multiple Locations
/// </summary>
[Table("PlatformLocation")]
[DisplayName("Location")]
public record PlatformLocation
{
    [Key]
    [Required]
    [DisplayName("Location ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;

    [Required]
    [MaxLength(128)]
    [DisplayName("Label")]
    [Comment("Label")]
    public string Label { get; set;} = string.Empty;

    [MaxLength(256)]
    [DisplayName("Description")]
    [Comment("Description")]
    public string? Description { get; set;}

    [DisplayName("Remark")]
    [Comment("Remark")]
    public string? Remark { get; set; }
}