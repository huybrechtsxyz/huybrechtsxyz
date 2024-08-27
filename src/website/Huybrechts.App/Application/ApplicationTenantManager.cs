using FluentResults;
using Hangfire;
using Huybrechts.App.Data;
using Huybrechts.Core.Application;
using Huybrechts.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Application;

public class ApplicationTenantManager
{
    public static ApplicationTenant NewTenant()
    {
        return new ApplicationTenant()
        {
            State = ApplicationTenantState.New
        };
    }

    public static bool AllowUpdatingTenant(ApplicationTenantState state) => state is not ApplicationTenantState.Removing and not ApplicationTenantState.Removed;

    // New -> Pending | Disabled  -> Pending
    public static bool AllowEnablingTenant(ApplicationTenantState state) => state is ApplicationTenantState.New or ApplicationTenantState.Disabled;

    // Pending -> Active
    public static bool AllowEnableTenant(ApplicationTenantState state) => state is ApplicationTenantState.Pending;

    // Active -> Disabling
    public static bool AllowDisablingTenant(ApplicationTenantState state) => state is ApplicationTenantState.Active;

    // Disabling -> Disabled
    public static bool AllowDisableTenant(ApplicationTenantState state) => state is ApplicationTenantState.Disabling;

    // New -> Removing | Disabled  -> Removing
    public static bool AllowRemovingTenant(ApplicationTenantState state) => state is ApplicationTenantState.New or ApplicationTenantState.Disabled;

    //Removing  -> Removed
    public static bool AllowRemoveTenant(ApplicationTenantState state) => state is ApplicationTenantState.Removing;

    public static string NormalizeIdentifier(string identifier) => (identifier ?? string.Empty).Trim().ToLowerInvariant();

    public static Result<ApplicationTenant> ValidateApplicationTenant(ApplicationTenant tenant)
    {
        var validationErrors = new List<ValidationResult>();
        if (Validator.TryValidateObject(tenant, new ValidationContext(tenant), validationErrors, validateAllProperties: true))
            return Result.Ok();

        Result result = new();
        foreach (var error in validationErrors)
        {
            result.WithError(error.ErrorMessage);
        }
        return result;
    }

    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationContext _dbcontext;
    private readonly ILogger<ApplicationTenantManager> _logger;

    private static Result ThrowRoleNotFound(string roleid) => Result.Fail(ApplicationLocalization.RoleNotFound.Replace("{0}", roleid));

    private static Result ThrowTenantNotFound(string tenantid) => Result.Fail(ApplicationLocalization.TenantNotFound.Replace("{0}", tenantid));

    private static Result ThrowUserNotFound(string userid) => Result.Fail(ApplicationLocalization.UserNotFound.Replace("{0}", userid));

    private static Result ThrowUserNotOwner(string user, string tenantId) => Result.Fail(ApplicationLocalization.UserNotFound.Replace("{0}", user).Replace("{1}", tenantId));

    public ApplicationTenantManager(
        ApplicationUserManager userManager,
        ApplicationRoleManager roleManager,
        ApplicationContext dbcontext,
        ILogger<ApplicationTenantManager> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbcontext = dbcontext;
        _logger = logger;
    }

    public async Task<IList<ApplicationTenant>> GetAllActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbcontext.ApplicationTenants.Where(q => q.State == ApplicationTenantState.Active).ToListAsync(cancellationToken);
    }

    public async Task<ICollection<ApplicationTenant>> GetTenantsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (user is null) return [];
        if (string.IsNullOrEmpty(user.Id)) return [];

        if (await _userManager.IsAdministratorAsync(user))
        {
            return await _dbcontext.ApplicationTenants.ToListAsync(cancellationToken);
        }

        var userId = user.Id;
        var query = from userRole in _dbcontext.UserRoles
                    join tenant in _dbcontext.ApplicationTenants on userRole.TenantId equals tenant.Id
                    where userRole.UserId.Equals(userId)
                    && tenant.State != ApplicationTenantState.Removed
                    select tenant;

        return await query.GroupBy(q => q.Name).Select(q => q.First()).ToListAsync(cancellationToken);
    }

	public async Task<ICollection<ApplicationTenant>> GetTenantsAsync(ApplicationUser user, ApplicationTenantState state, CancellationToken cancellationToken = default)
    {
        ApplicationTenantState[] states = [state];
        return await GetTenantsAsync(user, states, cancellationToken);
    }

	public async Task<ICollection<ApplicationTenant>> GetTenantsAsync(ApplicationUser user, IEnumerable<ApplicationTenantState> states, CancellationToken cancellationToken = default)
	{
		if (user is null) return [];
		if (string.IsNullOrEmpty(user.Id)) return [];

		if (await _userManager.IsAdministratorAsync(user))
		{
			return await _dbcontext.ApplicationTenants.ToListAsync(cancellationToken);
		}

        var userId = user.Id;
		var query = from userRole in _dbcontext.UserRoles
					join tenant in _dbcontext.ApplicationTenants on userRole.TenantId equals tenant.Id
					where userRole.UserId.Equals(userId) 
                    && (states == null || states.Contains(tenant.State))
					select tenant;

		return await query.GroupBy(q => q.Name).Select(q => q.First()).ToListAsync(cancellationToken);
	}

	public async Task<ApplicationTenant?> GetTenantAsync(ApplicationUser user, string tenantId)
    {
        var tenant = await _dbcontext.ApplicationTenants.FirstOrDefaultAsync(q => q.Id == tenantId);
        if (tenant == null)
            return null;

        if (await _userManager.IsInTenantAsync(user.Id, tenantId))
            return tenant;

        if (await _userManager.IsAdministratorAsync(user))
            return tenant;

        return null;
    }

    public async Task<ICollection<ApplicationRole>> GetTenantRolesAsync(ApplicationUser user, string tenantId)
    {
        return await _dbcontext.Roles.Where(q => q.TenantId == tenantId).ToListAsync() ?? [];
    }

    public async Task<ICollection<ApplicationUserRole>> GetTenantUserRolesAsync(ApplicationUser user, string tenantId, bool userOnly = false)
    {
        if (userOnly)
        {
            return await _dbcontext.UserRoles
                .Include(i => i.Role)
                .Include(i => i.User)
                .Where(q => q.UserId == user.Id && q.TenantId == tenantId).ToListAsync() ?? [];
        }

        return await _dbcontext.UserRoles
            .Include(i => i.Role)
            .Include(i => i.User)
            .Where(q => q.TenantId == tenantId).ToListAsync() ?? [];
    }

    public async Task<Result<ApplicationTenant>> CreateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        var currentUser = await _userManager.FindByIdAsync(user.Id);
        if (currentUser == null)
            return ThrowUserNotFound(user.Id);

        var result = ValidateApplicationTenant(tenant);
        if (result.IsFailed)
            return result;

        if (await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) is not null)
        {
            return Result.Fail(ApplicationLocalization.TenantAlreadyExists.Replace("{0}", tenant.Id));
        }

        ApplicationTenant appTenant = new()
        {
            Id = NormalizeIdentifier(tenant.Id),
            Name = tenant.Name,
            Description = tenant.Description,
            Remark = tenant.Remark,
            State = ApplicationTenantState.New,
            ConnectionString = null,
            DatabaseProvider = null
        };

        if (tenant.Picture is not null)
        {
            appTenant.Picture ??= new byte[tenant.Picture.Length];
            Array.Copy(tenant.Picture, appTenant.Picture, tenant.Picture.Length);
        }
        else
            appTenant.Picture = null;

        _dbcontext.ApplicationTenants.Add(appTenant);
        await _dbcontext.SaveChangesAsync();

        var roles = ApplicationRole.GetDefaultTenantRoles(appTenant.Id);
        foreach (var role in roles)
            await _roleManager.CreateAsync(role);

        var roleId = ApplicationRole.GetRoleName(appTenant.Id, ApplicationTenantRole.Owner);
        await _userManager.AddToRoleAsync(user, roleId);

        return Result.Ok(appTenant);
    }

    public async Task<Result<ApplicationTenant>> UpdateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        appTenant.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();
        return Result.Ok<ApplicationTenant>(appTenant);
    }

    public async Task<Result<ApplicationTenant>> DeleteTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        if (!AllowRemovingTenant(appTenant.State))
            return Result.Fail(ApplicationLocalization.TenantStateDelete.Replace("{0}", appTenant.State.GetName()));

        appTenant.State = ApplicationTenantState.Removing;
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<DeleteTenantWorker>(x => x.StartAsync(user.Id, appTenant.Id, default));

        return Result.Ok(appTenant);
    }

    public async Task<Result> CreateDefaultsForTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var currentUser = await _userManager.FindByIdAsync(user.Id);
        if (currentUser == null)
            return ThrowUserNotFound(user.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        BackgroundJob.Enqueue<CreateTenantWorker>(x => x.StartAsync(currentUser.Id, appTenant.Id, default));

        return Result.Ok();
    }

    public async Task<Result> EnableTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var currentUser = await _userManager.FindByIdAsync(user.Id);
        if (currentUser == null)
            return ThrowUserNotFound(user.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        if (!AllowEnablingTenant(appTenant.State))
            return Result.Fail(ApplicationLocalization.TenantStateActive.Replace("{0}", appTenant.State.GetName()));

        appTenant.State = ApplicationTenantState.Pending;
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<EnableTenantWorker>(x => x.StartAsync(currentUser.Id, appTenant.Id, default));

        return Result.Ok();
    }

    public async Task<Result> DisableTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var currentUser = await _userManager.FindByIdAsync(user.Id);
        if (currentUser == null)
            return ThrowUserNotFound(user.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        if (!AllowDisablingTenant(appTenant.State))
            return Result.Fail(ApplicationLocalization.TenantStateDisable.Replace("{0}", appTenant.State.GetName()));

        appTenant.State = ApplicationTenantState.Disabling;
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<DisableTenantWorker>(x => x.StartAsync(currentUser.Id, appTenant.Id, default));

        return Result.Ok();
    }

    public async Task<Result> AddUsersToTenantAsync(ApplicationUser user, string tenantId, string roleId, string[] usersToAdd)
    {
        if (!await _userManager.IsOwnerAsync(user, tenantId))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenantId);

        var currentUser = await _userManager.FindByIdAsync(user.Id);
        if (currentUser == null)
            return ThrowUserNotFound(user.Id);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenantId);
        if (appTenant is null)
            return ThrowTenantNotFound(tenantId);

        var appRole = await _roleManager.FindByIdAsync(roleId);
        if (appRole is null)
            return ThrowRoleNotFound(roleId);

        var ownerRole = await _roleManager.FindByNameAsync(ApplicationRole.GetRoleName(appTenant.Id, ApplicationTenantRole.Owner));
        if (ownerRole is null)
            return ThrowRoleNotFound(ApplicationRole.GetRoleName(appTenant.Id, ApplicationTenantRole.Owner));

        Result result = new();
        foreach (var item in usersToAdd)
        {
            var appUser = await _userManager.FindByEmailAsync(item);
            if (appUser is null)
            {
                result.WithError(ApplicationLocalization.UserNotFoundMail.Replace("{0}", item));
                continue;
            }

            var userRoles = (await GetTenantUserRolesAsync(appUser, appTenant.Id, true)).ToList();
            var deleteRoles = userRoles.Where(q => q.RoleId != appRole.Id && q.RoleId != ownerRole.Id).ToList();
            var hasOwnerRole = userRoles.Any(q => q.RoleId == ownerRole.Id);

            if (hasOwnerRole && appRole.Id != ownerRole.Id && !await _userManager.HasOtherOwnersAsync(appUser, appTenant.Id))
            {
                result.WithError(ApplicationLocalization.UserOnlyOwner.Replace("{0}", appUser.Email));

                continue;
            }

            var idResult = await _userManager.AddToRoleAsync(appUser!, appRole.NormalizedName!);
            if (idResult.Succeeded)
            {
                result.WithSuccess(ApplicationLocalization.UserAddedToRole
                    .Replace("{0}", appUser.Email)
                    .Replace("{1}", appRole.Label)
                    .Replace("{2}", appTenant.Name));
            }
            else
            {
                foreach (var idError in idResult.Errors)
                {
                    result.WithError(idError.Description);
                }
            }

            if (deleteRoles is not null && deleteRoles.Count > 0)
            {
                idResult = await _userManager.RemoveFromRolesAsync(appUser, deleteRoles.Select(s => s.Role.Name).ToArray()!);
                if (!idResult.Succeeded)
                {
                    foreach (var idError in idResult.Errors)
                    {
                        result.WithError(idError.Description);
                    }
                }
            }
        }

        return result;
    }

    public async Task<Result> RemoveUserFromTenantAsync(ApplicationUser user, string userId, string roleId, string tenantId)
    {
        if (!await _userManager.IsOwnerAsync(user, tenantId))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenantId);

        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser == null)
            return ThrowUserNotFound(userId);

        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenantId);
        if (appTenant is null)
            return ThrowTenantNotFound(tenantId);

        var appRole = await _roleManager.FindByIdAsync(roleId);
        if (appRole is null)
            return ThrowRoleNotFound(roleId);

        if ((await _userManager.IsOwnerAsync(appUser, tenantId))
            && (!await _userManager.HasOtherOwnersAsync(appUser, tenantId)))
        {
            return Result.Fail(ApplicationLocalization.UserOnlyOwner.Replace("{0}", appUser.Email));
        }

        var idResult = await _userManager.RemoveFromRoleAsync(appUser, appRole.NormalizedName!);
        if (idResult.Succeeded)
            return Result.Ok();

        Result result = new();
        foreach (var idError in idResult.Errors)
        {
            result.WithError(idError.Description);
        }
        return result;
    }

    public async Task<Result> TryCreateTenantAsync(ApplicationTenant tenant)
    {
        try
        {
            _dbcontext.ApplicationTenants.Add(tenant);
            await _dbcontext.SaveChangesAsync();
            return Result.Ok();
        }
        catch(Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<ApplicationTenant>> TryGetTenantAsync(string tenantId)
    {
        var tenant = await _dbcontext.ApplicationTenants.FirstOrDefaultAsync(q => q.Id == tenantId);
        if (tenant == null)
            return ThrowTenantNotFound(tenantId);

        return Result.Ok(tenant);
    }

    public async Task<Result> TryRemoveAsync(string tenantId)
    {
        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenantId);
        if (appTenant is null)
            return ThrowTenantNotFound(tenantId);

        if (!AllowDisablingTenant(appTenant.State))
            return Result.Fail(ApplicationLocalization.TenantStateDisable.Replace("{0}", appTenant.State.GetName()));

        appTenant.State = ApplicationTenantState.Disabling;
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<DisableTenantWorker>(x => x.StartAsync(string.Empty, appTenant.Id, default));

        return Result.Ok();
    }

    public async Task<Result> TryUpdateTenantAsync(ApplicationTenant tenant)
    {
        var appTenant = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (appTenant is null)
            return ThrowTenantNotFound(tenant.Id);

        appTenant.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(appTenant);
        await _dbcontext.SaveChangesAsync();
        return Result.Ok();
    }
}
