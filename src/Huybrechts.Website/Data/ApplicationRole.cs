using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

public enum ApplicationRoleEnum
{
	SystemAdmin
}

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
}
