using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityUserToken")]
public class ApplicationUserToken : IdentityUserToken<string>
{
}
