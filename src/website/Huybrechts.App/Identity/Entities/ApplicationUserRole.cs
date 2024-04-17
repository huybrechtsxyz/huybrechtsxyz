using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserRole")]
public sealed class ApplicationUserRole : IdentityUserRole<string>
{
	[NotMapped]
	public string TenantId => ApplicationRole.GetTenantId(RoleId);

	[NotMapped]
	public string Label => ApplicationRole.GetRoleLabel(RoleId);
}