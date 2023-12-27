using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityRoleClaim")]
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}
