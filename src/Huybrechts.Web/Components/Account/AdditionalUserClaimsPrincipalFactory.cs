using System.Security.Claims;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Huybrechts.Web.Components.Account;

public class AdditionalUserClaimsPrincipalFactory
     : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public AdditionalUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    { }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        ClaimsPrincipal principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        var claims = new List<Claim>
            {
                //new Claim(CustomClaimTypes.Planet, user.Planet)
            };
        //identity.AddClaims(claims);
        return principal;
    }
}