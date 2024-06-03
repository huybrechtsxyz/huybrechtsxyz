using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationUser")]
public sealed class ApplicationUser : IdentityUser
{
    [StringLength(128)]
    public string? GivenName { get; set; }

    [StringLength(128)]
    public string? Surname { get; set; }

    public byte[]? ProfilePicture { get; set; }

    [NotMapped]
    public string Fullname => GivenName + (GivenName?.Length > 0 && Surname?.Length > 0 ? " " : "") + Surname;
}