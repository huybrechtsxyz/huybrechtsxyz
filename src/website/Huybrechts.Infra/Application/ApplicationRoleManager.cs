﻿using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    private readonly ApplicationRoleStore _store;

    public ApplicationRoleManager(
        ApplicationRoleStore store, 
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
        ILogger<RoleManager<ApplicationRole>> logger) 
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
        _store = store;
    }

    public IQueryable<ApplicationTenant> Tenants => _store.Tenants;

    public override Task<IdentityResult> CreateAsync(ApplicationRole role)
    {
        role?.Set(role.Name!);
        return base.CreateAsync(role!);
    }

    public override Task<IdentityResult> SetRoleNameAsync(ApplicationRole role, string? name)
    {
        if (role is not null)
        {
            role.TenantId = ApplicationRole.GetTenant(role.Name!);
            role.Label = ApplicationRole.GetLabel(role.Name!);
        }
        return base.SetRoleNameAsync(role!, name);
    }

    public override Task<IdentityResult> UpdateAsync(ApplicationRole role)
    {
        role?.Set(role.Name!);
        return base.UpdateAsync(role!);
    }

    public override Task UpdateNormalizedRoleNameAsync(ApplicationRole role)
    {
        if (role is not null)
        {
            role.TenantId = ApplicationRole.GetTenant(role.Name!);
            role.Label = ApplicationRole.GetLabel(role.Name!);
        }
        return base.UpdateNormalizedRoleNameAsync(role!);
    }
}
