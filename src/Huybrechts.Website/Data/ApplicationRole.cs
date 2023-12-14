using Microsoft.AspNetCore.Identity;

namespace Huybrechts.Website.Data
{
	public class ApplicationRole : IdentityRole<Guid>
	{
		public static string Administrator => "Administrator";

		public static string User => "User";

		public string? Description { get; set; }

		public ApplicationRole() : base() { }

		public ApplicationRole(string roleName) : base(roleName) { }
	}
}
