using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserTenant")]
[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
public sealed class ApplicationUserTenant
{
    [Key]
    public int Id { get; set; } = 0;

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string TenantId { get; set; } = string.Empty;

    public string? Remark { get; set; }

    [Timestamp]
    public string? ConcurrencyStamp { get; set; }
}