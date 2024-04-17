using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationRoleClaim")]
public sealed class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}