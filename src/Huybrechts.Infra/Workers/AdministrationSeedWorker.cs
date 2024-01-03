using Huybrechts.Infra.Config;
using Huybrechts.Infra.Data;
using Huybrechts.Infra.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Huybrechts.Infra.Workers;

public class AdministrationSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private AdministrationContext? _dbcontext = null;
    private ApplicationUserManager _userManager;
    private ApplicationRoleManager _roleManager;

    private readonly ILogger _logger = Log.Logger.ForContext<AdministrationSeedWorker>();

    public AdministrationSeedWorker(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...");
        ApplicationSettings applicationSettings = new(_configuration);
        var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
            throw new Exception("The WebHostEnvironment service was not registered as a service");

        using var scope = _serviceProvider.CreateScope();
        _dbcontext = scope.ServiceProvider.GetRequiredService<AdministrationContext>() ??
            throw new Exception("The DatabaseContext service was not registered as a service");

        _userManager = (ApplicationUserManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationUserManager));
        _roleManager = (ApplicationRoleManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationRoleManager));

        if (environment.IsDevelopment() && applicationSettings.DoResetEnvironment())
        {
            _logger.Warning("Running database initializer...deleting existing database");
            await _dbcontext.Database.EnsureDeletedAsync(cancellationToken);
        }

        _logger.Information("Running database initializer...applying database migrations");
        await _dbcontext.Database.MigrateAsync(cancellationToken);

        await InitializeForAllAsync(cancellationToken);

        if (environment.IsDevelopment())
            await InitializeForDevelopmentAsync(cancellationToken);

        else if (environment.IsStaging())
            await InitializeForStagingAsync(cancellationToken);

        else if (environment.IsProduction())
            await InitializeForProductionAsync(cancellationToken);

        _logger.Information("Running database initializer...loading tenants");
        await InitializeTenantsAsync(scope, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task InitializeForAllAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...creating users and roles");
        var vincent = await CreateDefaultUsers();

        _logger.Information("Running database initializer...creating tenants");

        //await CreateDefaultApplicationRoles();

        /*
		if (user is null)
		{
			await _userManager.CreateAsync(sysadminUser, "password");
			await _userManager.AddToRoleAsync(sysadminUser, ApplicationRoleEnum.SystemAdmin.ToString());
		}
		*/
    }

    private async Task InitializeForDevelopmentAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...applying development migrations");
    }

    private async Task InitializeForStagingAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...applying staging migrations");
    }

    private async Task InitializeForProductionAsync(CancellationToken cancellationToken)
    {
        _logger.Warning("Running database initializer...applying productions migrations");
    }

    private async Task InitializeTenantsAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        StdSchedulerFactory factory = new();
        IScheduler scheduler = await factory.GetScheduler(cancellationToken);
        var collection = (TenantContextCollection)scope.ServiceProvider.GetRequiredService(typeof(ITenantContextCollection));
        TenantContextFactory tenantFactory = new();
        tenantFactory.BuildTenantCollection(_dbcontext!, collection);
        foreach (var tenant in collection)
        {
            await ScheduleTenantUpdateAsync(scheduler, (TenantContext)tenant, cancellationToken);
        }
    }

    private async Task<ApplicationUser?> CreateDefaultUsers()
    {
        _logger.Information("Running database initializer...creating default users");

        var vincent = await CreateUser(new ApplicationUser()
        {
            UserName = "vincent@huybrechts.xyz",
            Email = "vincent@huybrechts.xyz",
            GivenName = "Vincent",
            Surname = "Huybrechts",
            EmailConfirmed = true
        });
        if (vincent is null)
            _logger.Information("Running database initializer...error creating users");

        var sysadmin = await CreateRole(new ApplicationRole()
        {
            Name = ApplicationRole.SystemAdministrator
        });
        if (sysadmin is null)
            _logger.Information("Running database initializer...error creating roles");

        var user = await CreateRole(new ApplicationRole()
        {
            Name = ApplicationRole.SystemUser
        });
        if (user is null)
            _logger.Information("Running database initializer...error creating roles");

        if (vincent is not null && sysadmin is not null)
        {
            var result = await _userManager.AddToRoleAsync(vincent, sysadmin.Name!);
            if (result.Succeeded)
                return vincent;

            foreach (var error in result.Errors)
                _logger.Error("Error adding role {role} to user {user}: {errorcode} with {errortext}", sysadmin.Name, vincent.Email, error.Code, error.Description);
        }

        return vincent;
    }

    private async Task<ApplicationRole?> CreateRole(ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(() => role);
        ArgumentNullException.ThrowIfNull(() => role.Name);
        var item = await _roleManager.FindByNameAsync(role.Name!);
        if (item is not null)
            return item;

        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
            return await _roleManager.FindByNameAsync(role.Name!);

        foreach (var error in result.Errors)
            _logger.Error("Error creating default role {role}: {errorcode} with {errortext}", role.Name, error.Code, error.Description);

        return null;
    }

    private async Task<ApplicationUser?> CreateUser(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(nameof(user));
        ArgumentNullException.ThrowIfNull(nameof(user.UserName));
        ArgumentNullException.ThrowIfNull(nameof(user.Email));
        var item = await _userManager.FindByEmailAsync(user.Email!);
        if (item is not null)
            return item;

        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
            return await _userManager.FindByEmailAsync(user.Email!);

        foreach (var error in result.Errors)
            _logger.Error("Error creating default user {user}: {errorcode} with {errortext}", user.Email, error.Code, error.Description);

        return null;
    }

    private async Task ScheduleTenantUpdateAsync(IScheduler scheduler, TenantContext tenant, CancellationToken cancellationToken)
    {
        var job = JobBuilder.Create<TenantUpdateJob>()
            .WithIdentity(TenantUpdateJob.Key)
            .UsingJobData("tenantid", tenant.Tenant.Id)
            .Build();

        var trigger = TriggerBuilder.Create().WithIdentity("tenant-" + tenant.Tenant.Id, "identity").StartNow().Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}

/*
    private async Task CreateDefaultApplicationRoles()
	{
		if (_roleManager is not null)
		{
			var defaultRoles = ApplicationRole.GetDefaultRoles();
			foreach (var defaultRole in defaultRoles)
			{
				if (!await _roleManager.RoleExistsAsync(defaultRole.Name!))
				{
					await _roleManager.CreateAsync(new ApplicationRole()
					{
						Name = defaultRole.Name!
					});
				}
			}
		}
	}
*/