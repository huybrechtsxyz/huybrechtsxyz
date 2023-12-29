using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityTenant")]
[Index(nameof(Code), IsUnique = true)]
public record ApplicationTenant
{
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public string? Remark { get; set; }

    public byte[]? ProfilePicture { get; set; }

    [StringLength(512)]
    public string ConnectionString { get; set; } = string.Empty;



    [Timestamp]
    public byte[]? ConcurrencyToken { get; set; }

    public ICollection<ApplicationTenantRole>? Roles { get; set; }

    public ICollection<ApplicationTenantUser>? Users { get; set; }
}
