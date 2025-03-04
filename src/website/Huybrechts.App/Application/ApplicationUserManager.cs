using FluentResults;
using Hangfire;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Huybrechts.App.Application;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    private readonly ApplicationUserStore UserStore;

    private static Result ReturnUserNotFound(string userid) => Result.Fail(ApplicationLocalization.UserNotFound.Replace("{0}", userid));

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

    public async Task<Result> DeletePersonalInfo(ApplicationUser user)
    {
        try
        {
            var appUser = await FindByIdAsync(user.Id);
            if (appUser == null)
            {
                return ReturnUserNotFound(user.Id);
            }

            BackgroundJob.Enqueue<DeletePersonalInfoWorker>(x => x.StartAsync(user.Id));

            return Result.Ok();
        }
        catch(Exception ex)
        {
            return Result.Fail(ex.Message);
        }
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
	/// Retrieves the roles the specified <paramref name="user"/> is a member of.
	/// </summary>
	/// <param name="user">The user whose roles should be retrieved.</param
	/// <param name="tenantId">The tenant Id for filtering the roles</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
	/// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
	public Task<IList<string>> GetRolesAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
	{
        return UserStore.GetRolesAsync(user, tenantId, cancellationToken);
	}

	/// <summary>
	/// Get the single tenant from the database
	/// </summary>
	/// <param name="user">The user whose roles should be retrieved.</param>
	/// <param name="tenantId">The tenant Id for filtering the roles</param>
	/// <returns>Returns the tenant if the user has access to it or is an administrator</returns>
	public async Task<ApplicationTenant?> GetTenantAsync(ApplicationUser user, string tenantId)
	{
        return await UserStore.GetTenantAsync(user, tenantId);
	}

    /// <summary>
    /// Does the tenant have other users that are owner for this tenant
    /// </summary>
    /// <param name="user">The user to check against</param>
    /// <param name="tenantId">The tenant to check against</param>
    /// <param name="cancellationToken"></param>
    /// <returns>If other owners are registered to this tenant</returns>
    public async Task<bool> HasOtherOwnersAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        return await UserStore.HasOtherOwnersAsync(user, tenantId, cancellationToken);
    }

    /// <summary>
    /// Is the user an adminsitrator?
    /// </summary>
    /// <param name="user">The user for who to check adminsitrator priviledges.</param>
    /// <returns>True if administrator</returns>
    public virtual async Task<bool> IsAdministratorAsync(ApplicationUser user)
    {
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(ApplicationSystemRole.Administrator));
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
        return await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenantId, ApplicationTenantRole.Owner));
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
        if (await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenant, ApplicationTenantRole.Owner)))
            return true;
        if (await IsInRoleAsync(user, ApplicationRole.GetRoleName(tenant, ApplicationTenantRole.Manager)))
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
    public async Task<bool> IsInTenantAsync(string userId, string tenantId)
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
            throw new ApplicationException(ApplicationLocalization.UserNotFound.Replace("{0}", user.Id));
        item.GivenName = givenName;
        item.Surname = surname;
        await base.UpdateSecurityStampAsync(item).ConfigureAwait(false);
        return await UpdateUserAsync(user).ConfigureAwait(false);
    }
}