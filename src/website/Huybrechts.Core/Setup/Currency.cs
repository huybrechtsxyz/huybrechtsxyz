using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

[Table("SetupCurrency")]
[DisplayName("Currency")]
public record Currency : Entity, IEntity
{
    [Required]
    [MaxLength(10)]
    [DisplayName("Currency")]
    [Comment("Primary Key")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(256)]
    [DisplayName("Description")]
    [Comment("Description")]
    public string? Description { get; set;}
}