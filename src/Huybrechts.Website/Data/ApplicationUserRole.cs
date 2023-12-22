using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityUserRole")]
public class ApplicationUserRole : IdentityUserRole<string>
{
}
