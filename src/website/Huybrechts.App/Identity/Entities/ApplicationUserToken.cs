using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserToken")]
public sealed class ApplicationUserToken : IdentityUserToken<string>
{
}