using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationUserRole")]
[PrimaryKey(nameof(UserId), nameof(RoleId))]
public sealed class ApplicationUserRole : IdentityUserRole<string>
{
	[NotMapped]
	public string TenantId => ApplicationRole.GetTenantId(RoleId);

	[NotMapped]
	public string Label => ApplicationRole.GetRoleLabel(RoleId);
}