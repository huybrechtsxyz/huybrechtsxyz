using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
	public enum DefaultRole
	{
		None = 0,
		User = 1,
		Sysadmin = 64
	}

	public static List<ApplicationRole> GetDefaultRoles()
	{
		List<ApplicationRole> list = [];
		foreach (var value in Enum.GetValues(typeof(DefaultRole)).Cast<DefaultRole>())
		{
			var item = new ApplicationRole()
			{
				Name = value.ToString()
			};
			list.Add(item);
		}
		return list;
	}
}
