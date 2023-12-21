using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityRoleClaim")]
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}
