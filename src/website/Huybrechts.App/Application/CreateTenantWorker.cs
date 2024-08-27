using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Huybrechts.App.Application;

public class CreateTenantWorker
{
    private readonly IMediator _mediator;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationContext _applicationContext;
    private readonly FeatureContext _featureContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CreateTenantWorker(
        IMediator mediator,
        ApplicationUserManager userManager,
        ApplicationContext applicationContext,
        FeatureContext featureContext,
        IWebHostEnvironment webHostEnvironment)
    {
        _mediator = mediator;
        _userManager = userManager;
        _applicationContext = applicationContext;
        _featureContext = featureContext;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task StartAsync(string userId, string tenantId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        _ = await _userManager.FindByIdAsync(userId) ??
            throw new InvalidOperationException($"Unable to find user with ID {userId}.");

        var tenant = await _applicationContext.ApplicationTenants.FindAsync([tenantId], cancellationToken: token);
        if (tenant is null)
            return;
    }
}
