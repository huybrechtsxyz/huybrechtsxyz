using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityUser")]
public class ApplicationUser : IdentityUser
{
    [StringLength(128)]
    public string? FirstName { get; set; }

    [StringLength(128)]
    public string? LastName { get; set; }

	public byte[]? ProfilePicture { get; set; }

	[NotMapped]
    public string Fullname => FirstName + (FirstName?.Length > 0 && LastName?.Length > 0 ? " " : "") + LastName;
}
