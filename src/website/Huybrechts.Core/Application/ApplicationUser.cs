using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
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
    public string Fullname
    {
        get
        {
            if (!string.IsNullOrEmpty(GivenName) || !string.IsNullOrEmpty(Surname))
                return (GivenName ?? string.Empty) + (GivenName?.Length > 0 && Surname?.Length > 0 ? " " : "") + (Surname ?? string.Empty);
            return string.Empty;
        }
    }

    public ICollection<ApplicationUserClaim> Claims { get; set; } = [];
    public ICollection<ApplicationUserLogin> Logins { get; set; } = [];
    public ICollection<ApplicationUserToken> Tokens { get; set; } = [];
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}