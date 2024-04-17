using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("IdentityRoleClaim")]
public sealed class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}