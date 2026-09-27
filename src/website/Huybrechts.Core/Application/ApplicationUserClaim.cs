using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationUserClaim")]
public sealed class ApplicationUserClaim : IdentityUserClaim<string>
{
    public ApplicationUser User { get; set; } = new();
}