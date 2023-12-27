using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityUserLogin")]
public class ApplicationUserLogin : IdentityUserLogin<string>
{
}
