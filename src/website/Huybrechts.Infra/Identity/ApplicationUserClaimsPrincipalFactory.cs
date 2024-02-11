using Huybrechts.Core.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Huybrechts.Infra.Identity;

public class ApplicationUserClaimsPrincipalFactory :
    UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>,
    IUserClaimsPrincipalFactory<ApplicationUser>
{
    public ApplicationUserClaimsPrincipalFactory(
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        ClaimsPrincipal principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        var claims = new List<Claim> { };
        if (!string.IsNullOrEmpty(user.GivenName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.GivenName));
        if (!string.IsNullOrEmpty(user.Surname))
            claims.Add(new Claim(ClaimTypes.Surname, user.Surname));
        identity.AddClaims(claims);
        return principal;
    }
}