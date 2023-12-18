using Microsoft.AspNetCore.Identity;

namespace Huybrechts.Website.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
	{
        public string? FirstName { get; set; }

		public string? LastName { get; set; }

		public string Fullname => FirstName + (FirstName?.Length > 0 && LastName?.Length > 0 ? " " : "") + LastName;

		public string? ImageType { get; set; }

		public string? ImageData { get; set; }

		public string? ImageData32 { get; set; }

		public string? ImageData64 { get; set; }
	}
}