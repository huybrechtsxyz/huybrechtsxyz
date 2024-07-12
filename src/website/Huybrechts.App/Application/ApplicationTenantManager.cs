using FluentResults;
using Hangfire;
using Huybrechts.App.Data;
using Huybrechts.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

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

    public static string NormalizeIdentifier(string identifier) => (identifier ?? string.Empty).Trim().ToLowerInvariant();

    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationContext _dbcontext;
    private readonly ILogger<ApplicationTenantManager> _logger;

    private static Result ThrowTenantNotFound(string tenantid) => Result.Fail($"Unable to load tenant with ID '{tenantid}'.");

    private static Result ThrowUserNotFound(string userid) => Result.Fail($"Unable to load user with ID '{userid}'.");

    private static Result ThrowUserNotOwner(string user, string tenantId) => Result.Fail($"User '{user}' is not the owner of the tenant '{tenantId}'");

    private static Result<ApplicationTenant> ValidateApplicationTenant(ApplicationTenant tenant)
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

    public async Task<ICollection<ApplicationTenant>> GetTenantsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (user is null) return [];
        if (string.IsNullOrEmpty(user.Id)) return [];

        if (await _userManager.IsAdministratorAsync(user))
        {
            return await _dbcontext.ApplicationTenants.ToListAsync();
        }

        var userId = user.Id;
        var query = from userRole in _dbcontext.UserRoles
                    join tenant in _dbcontext.ApplicationTenants on userRole.TenantId equals tenant.Id
                    where userRole.UserId.Equals(userId) && tenant.State != ApplicationTenantState.Removed
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

    public async Task<ICollection<ApplicationUserRole>> GetTenantUserRolesAsync(ApplicationUser user, string tenantId)
    {
        return await _dbcontext.UserRoles
            .Include(i => i.Role)
            .Include(i => i.User)
            .Where(q => q.TenantId == tenantId).ToListAsync() ?? [];
    }

    public async Task<Result<ApplicationTenant>> CreateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id);
        if (appUser == null)
            return ThrowUserNotFound(user.Id);

        var result = ValidateApplicationTenant(tenant);
        if (result.IsFailed)
            return result;

        if (await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) is not null)
        {
            return Result.Fail($"Tenant with Id {tenant.Id} already exists");
        }

        ApplicationTenant item = new()
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
            item.Picture ??= new byte[tenant.Picture.Length];
            Array.Copy(tenant.Picture, item.Picture, tenant.Picture.Length);
        }
        else
            item.Picture = null;

        _dbcontext.ApplicationTenants.Add(item);
        await _dbcontext.SaveChangesAsync();

        var roles = ApplicationRole.GetDefaultTenantRoles(item.Id);
        foreach (var role in roles)
            await _roleManager.CreateAsync(role);

        var roleId = ApplicationRole.GetRoleName(item.Id, ApplicationDefaultTenantRole.Owner);
        await _userManager.AddToRoleAsync(user, roleId);
        return Result.Ok<ApplicationTenant>(item);
    }

    public async Task<Result<ApplicationTenant>> UpdateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (item is null)
            return ThrowTenantNotFound(tenant.Id);

        item.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
        return Result.Ok<ApplicationTenant>(item);
    }

    public async Task<Result<ApplicationTenant>> DeleteTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            return ThrowUserNotOwner(user.NormalizedUserName!, tenant.Id);

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id);
        if (item is null)
            return ThrowTenantNotFound(tenant.Id);

        item.State = ApplicationTenantState.Removing;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<DeleteTenantInfoWorker>(x => x.StartAsync(user.Id, item.Id, default));

        return Result.Ok<ApplicationTenant>(item);
    }
}
