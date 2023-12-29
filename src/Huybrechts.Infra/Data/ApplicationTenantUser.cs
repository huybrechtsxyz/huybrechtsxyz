using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityTenantUser")]
[Index(nameof(ApplicationTenantCode), nameof(ApplicationUserId), IsUnique = true)]
public record ApplicationTenantUser
{
    [Key]
    [StringLength(450)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string ApplicationTenantCode { get; set; } = string.Empty;

    [Required]
    [StringLength(450)]
    public string ApplicationUserId { get; set; } = string.Empty;

    public string? Remark { get; set; }

    [Timestamp]
    public byte[]? ConcurrencyToken { get; set; }

    public ICollection<ApplicationTenantUserRole>? UserRoles { get; set; }
}
