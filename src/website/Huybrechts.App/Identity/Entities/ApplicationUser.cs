using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUser")]
public sealed class ApplicationUser : IdentityUser
{
    [StringLength(128)]
    public string? GivenName { get; set; }

    [StringLength(128)]
    public string? Surname { get; set; }

    public byte[]? ProfilePicture { get; set; }

    [NotMapped]
    public string Fullname => GivenName + (GivenName?.Length > 0 && Surname?.Length > 0 ? " " : "") + Surname;

    public ICollection<ApplicationUserTenant> Tenants { get; set; } = new List<ApplicationUserTenant>();
}