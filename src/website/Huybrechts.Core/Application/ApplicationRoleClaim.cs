using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationRoleClaim")]
public sealed class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}