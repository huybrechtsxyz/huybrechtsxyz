using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace Huybrechts.App.Identity;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationUserStore UserStore;

	public ApplicationUserManager(
        //IUserStore<ApplicationUser> store,
        ApplicationUserStore store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<ApplicationUserManager> logger) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        UserStore = (ApplicationUserStore)store;
    }

    public async Task<IdentityResult> AddToTenantAsync(ApplicationUser user, string tenantId, string roleName)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var normalizedRole = NormalizeName(ApplicationRole.GetTenantRole(tenantId, roleName));

        bool inTenant = await UserStore.IsInTenantAsync(user, tenantId, CancellationToken);
        bool inRole = await UserStore.IsInRoleAsync(user, normalizedRole, CancellationToken);

        if (inTenant && inRole)
            return IdentityResult.Failed([new IdentityError() { Code = "UserInTenant", Description = $"User already member of tenant {tenantId} for role {roleName}" }]);

        if (!inTenant)
            await UserStore.AddToTenantAsync(user, tenantId, cancellationToken: CancellationToken);

        if (!inRole)
            await base.AddToRoleAsync(user, normalizedRole);

        await UserStore.UpdateAsync(user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> AddToTenantAsync(ApplicationUser user, string tenantId, IEnumerable<string> roles)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(roles);

        if (!await UserStore.IsInTenantAsync(user, tenantId, CancellationToken))
            await UserStore.AddToTenantAsync(user, tenantId, cancellationToken: CancellationToken);

        List<string> roleList = [];
        foreach (var role in roles)
            roleList.Add(NormalizeName(ApplicationRole.GetTenantRole(tenantId, role)));

        var result = await AddToRolesAsync(user, roleList);
        if (!result.Succeeded)
            return result;

        await UserStore.UpdateAsync(user);
        return IdentityResult.Success;
    }

    public async Task<IList<string>> GetTenantsAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        return await UserStore.GetTenantsAsync(user, CancellationToken);
    }

    public async Task<IList<ApplicationTenant>> GetApplicationTenantsAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        return await UserStore.GetApplicationTenantsAsync(user, CancellationToken);
    }

    public async Task<IList<ApplicationUser>> GetUsersInTenantAsync(string tenantId)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(tenantId);
        return await UserStore.GetUsersInTenantAsync(tenantId, CancellationToken);
    }

    public async Task<bool> IsInTenantAsync(ApplicationUser user, string tenantId)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);
        return await UserStore.IsInTenantAsync(user, tenantId, CancellationToken);
    }

    public async Task<IdentityResult> RemoveFromTenantAsync(ApplicationUser user, string tenantId)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        List<IdentityError> errors = [];
        if (!await UserStore.IsInTenantAsync(user, tenantId, CancellationToken))
        {
            errors.Add(new() { Code = "UserNotInTenant", Description = $"User {user.Id} is not part of tenant {tenantId}"});
            return IdentityResult.Failed([.. errors]);
        }

        await UserStore.RemoveFromTenantAsync(user,tenantId, CancellationToken); //also deletes linked tenant roles
        await UserStore.UpdateAsync(user);
        return IdentityResult.Success;
    }
}