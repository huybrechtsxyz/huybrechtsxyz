using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationUserRole")]
[PrimaryKey(nameof(UserId), nameof(RoleId))]
public sealed class ApplicationUserRole : IdentityUserRole<string>
{
    //TenantId => ApplicationRole.GetTenantId(RoleId);
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string? TenantId { get; set; }

    //Label => ApplicationRole.GetRoleLabel(RoleId);
    [Required]
    [StringLength(256)]
    public string Label { get; set; } = string.Empty;
}