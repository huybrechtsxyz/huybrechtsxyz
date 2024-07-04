using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationUserLogin")]
[PrimaryKey(nameof(LoginProvider), nameof(ProviderKey))]
public sealed class ApplicationUserLogin : IdentityUserLogin<string>
{
    public ApplicationUser User { get; set; } = new();
}