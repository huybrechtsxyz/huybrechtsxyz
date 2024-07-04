using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationUserToken")]
[PrimaryKey(nameof(UserId), nameof(LoginProvider), nameof(Name))]
public sealed class ApplicationUserToken : IdentityUserToken<string>
{
    public ApplicationUser User { get; set; } = new();
}