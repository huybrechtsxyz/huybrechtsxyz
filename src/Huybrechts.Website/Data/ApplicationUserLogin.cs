using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityUserLogin")]
public class ApplicationUserLogin : IdentityUserLogin<string>
{
}
