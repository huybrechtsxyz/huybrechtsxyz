﻿using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Services.Mail;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Application;

public class DisableTenantWorker
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationContext _dbcontext;

    public DisableTenantWorker(ApplicationUserManager userManager, ApplicationContext dbcontext)
    {
        _userManager = userManager;
        _dbcontext = dbcontext;
    }

    public async Task StartAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        _ = await _userManager.FindByIdAsync(userId) ??
            throw new InvalidOperationException($"Unable to find user {userId}.");

        var tenant = await _dbcontext.ApplicationTenants.FindAsync([tenantId], cancellationToken: cancellationToken);
        if (tenant is null)
            return;

        if (!ApplicationTenantManager.AllowDisableTenant(tenant.State))
            return;

        tenant.State = ApplicationTenantState.Disabled;
        _dbcontext.ApplicationTenants.Update(tenant);
        await _dbcontext.SaveChangesAsync(cancellationToken);
    }
}
