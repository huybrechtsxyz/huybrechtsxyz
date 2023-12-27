using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityUserRole")]
public class ApplicationUserRole : IdentityUserRole<string>
{
}
