﻿using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Huybrechts.Infra.Application;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationUserStore UserStore;

	public ApplicationUserManager(
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

    /// <summary>
    /// Is the user an adminsitrator?
    /// </summary>
    /// <param name="user">The user for who to check adminsitrator priviledges.</param>
    /// <returns>True if administrator</returns>
    public virtual async Task<bool> IsAdministratorAsync(ApplicationUser user)
    {
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(ApplicationDefaultSystemRole.Administrator));
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the tenant owners.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
    /// a member of the administrators.
    /// </returns>
    public virtual async Task<bool> IsOwnerAsync(ApplicationUser user, string tenantId)
    {
        if (await IsAdministratorAsync(user))
            return true;
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenantId, ApplicationDefaultTenantRole.Owner));
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the tenant managers.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
    /// a member of the administrators.
    /// </returns>
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

    /// <summary>
    /// Is the current user a member of the tenant?
    /// </summary>
    /// <param name="userId">The user for who to check if they are in a tenant</param>
    /// <param name="tenantId">The tenant to check </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True is the user is a member of the tenant</returns>
    public async Task<bool> IsInTenantAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        CancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);
        return await UserStore.IsInTenantAsync(userId, tenantId, CancellationToken);
    }

    /// <summary>
    /// Sets the phone number for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose phone number to set.</param>
    /// <param name="phoneNumber">The phone number to set.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
    /// of the operation.
    /// </returns>
    public virtual async Task<IdentityResult> SetGivenSurNameAsync(ApplicationUser user, string? givenName, string? surname)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var item = await UserStore.FindByIdAsync(user.Id) ??
            throw new ApplicationException($"User with id {user.Id} was not found!");
        item.GivenName = givenName;
        item.Surname = surname;
        await base.UpdateSecurityStampAsync(item).ConfigureAwait(false);
        return await UpdateUserAsync(user).ConfigureAwait(false);
    }
}