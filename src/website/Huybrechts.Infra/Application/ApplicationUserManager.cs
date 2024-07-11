using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;

namespace Huybrechts.Infra.Application;

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
        UserStore = store;
    }

    /// <summary>
    /// Gets a list of role names the specified <paramref name="user"/> belongs to.
    /// </summary>
    /// <param name="user">The user whose role names to retrieve.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
    public async Task<IList<ApplicationRole>> GetApplicationRolesAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return await UserStore.GetApplicationRolesAsync(user, CancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<string>> GetTenantNamesAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        return await UserStore.GetTenantNamesAsync(user, CancellationToken);
    }

    public async Task<IList<ApplicationTenant>> GetTenantsListAsync(ApplicationUser user)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        return await UserStore.GetTenantsListAsync(user, CancellationToken);
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the administrators.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
    /// a member of the administrators.
    /// </returns>
    public virtual async Task<bool> IsAdministratorAsync(ApplicationUser user)
    {
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(ApplicationDefaultSystemRole.Administrator));
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the administrators.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
    /// a member of the administrators.
    /// </returns>
    public virtual async Task<bool> IsOwnerAsync(ApplicationUser user, string tenant)
    {
        if (await IsAdministratorAsync(user))
            return true;
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenant, ApplicationDefaultTenantRole.Owner));
    }

    public virtual async Task<bool> IsManagerAsync(ApplicationUser user, string tenant)
    {
        if (await IsAdministratorAsync(user))
            return true;
        if (await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenant, ApplicationDefaultTenantRole.Owner)))
            return true;
        if (await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenant, ApplicationDefaultTenantRole.Manager)))
            return true;
        return false;
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

    public async Task<IdentityResult> UpdateGivenSurNameAsync(string userid, string given, string sur)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(given);
        ArgumentNullException.ThrowIfNull(sur);
        var user = await UserStore.GetUserAsync(userid);
        if (user is null)
            return IdentityResult.Failed([new IdentityError() { Description = "Invalid user" }]);

        user.GivenName = given;
        user.Surname = sur;
        return await UserStore.UpdateAsync(user);
    }
}