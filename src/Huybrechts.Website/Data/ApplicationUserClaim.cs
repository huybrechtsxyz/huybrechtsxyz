using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Website.Data;

[Table("IdentityUserClaim")]
public class ApplicationUserClaim : IdentityUserClaim<string>
{
}
