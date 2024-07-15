using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// The Projects.
/// </summary>
[Table("Project")]
[DisplayName("Project")]
public record Project
{
    [Key]
    [Required]
    [DisplayName("Project ID")]
    [Comment("Primary Key")]
    public int Id { get; set;} = 0;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;

    [MaxLength(256)]
    [DisplayName("Description")]
    [Comment("Description")]
    public string? Description { get; set;}

    [MaxLength(256)]
    [DisplayName("Tags")]
    [Comment("Tags (semicolon separated)")]
    public string? Tags { get; set;}

    [DisplayName("Remark")]
    [Comment("Remark")]
    public string? Remark { get; set; }

    [Required]
    [MaxLength(10)]
    [DisplayName("Currency Code")]
    [Comment("Project currency")]
    public string CurrencyCode { get; set; } = string.Empty;
}