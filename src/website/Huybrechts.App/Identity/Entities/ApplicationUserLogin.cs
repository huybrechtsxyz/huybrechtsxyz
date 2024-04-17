using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityUserLogin")]
public sealed class ApplicationUserLogin : IdentityUserLogin<string>
{
}