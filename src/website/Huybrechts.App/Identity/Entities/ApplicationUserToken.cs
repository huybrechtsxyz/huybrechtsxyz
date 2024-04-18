using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationUserToken")]
[PrimaryKey(nameof(UserId), nameof(LoginProvider), nameof(Name))]
public sealed class ApplicationUserToken : IdentityUserToken<string>
{
}