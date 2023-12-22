using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
}
