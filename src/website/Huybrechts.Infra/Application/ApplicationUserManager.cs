using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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