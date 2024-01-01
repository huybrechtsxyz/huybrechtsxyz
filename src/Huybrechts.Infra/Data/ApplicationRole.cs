using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
	public static List<ApplicationRole> GetDefaultRoles()
	{
		List<ApplicationRole> list = [];
		foreach (var value in Enum.GetValues(typeof(ApplicationRoleValues)).Cast<ApplicationRoleValues>())
		{
			var item = new ApplicationRole()
			{
				Name = value.ToString()
			};
			if (value != ApplicationRoleValues.None)
				list.Add(item);
		}
		return list;
	}
}

public enum ApplicationRoleValues
{
    None = 0,
    User = 1,
    Sysadmin = 128
}