using Huybrechts.App.Config;
using Huybrechts.App.Extensions;
using Huybrechts.App.Identity;
using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.App.Data.Workers;

public class DatabaseSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly ApplicationSettings _applicationSettings;

	private ApplicationContext? _dbcontext = null;
    private ApplicationUserManager? _userManager;
    private ApplicationRoleManager? _roleManager;

    private readonly ILogger _logger = Log.Logger.ForContext<DatabaseSeedWorker>();

    public DatabaseSeedWorker(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _applicationSettings = new(configuration);
	}

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...");
        var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
            throw new Exception("The WebHostEnvironment service was not registered as a service");

        using var scope = _serviceProvider.CreateScope();
        _dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationContext>() ??
            throw new Exception("The DatabaseContext service was not registered as a service");

        _userManager = (ApplicationUserManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationUserManager));
        _roleManager = (ApplicationRoleManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationRoleManager));

        if ((environment.IsDevelopment() || environment.IsTest() ) && _applicationSettings.DoResetEnvironment())
        {
            _logger.Warning("Running database initializer...deleting existing database");
            await _dbcontext.Database.EnsureDeletedAsync(cancellationToken);
        }

        _logger.Information("Running database initializer...applying database migrations");
        await _dbcontext.Database.MigrateAsync(cancellationToken);

        await InitializeForAllAsync(cancellationToken);

        //if (environment.IsDevelopment())
        //    await InitializeForDevelopmentAsync(cancellationToken);

        //if (environment.IsTest())
        //    await InitializeForTestAsync(cancellationToken);

        //else if (environment.IsStaging())
        //    await InitializeForStagingAsync(cancellationToken);

        //else if (environment.IsProduction())
        //    await InitializeForProductionAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task InitializeForAllAsync(CancellationToken cancellationToken)
    {
        var vincent = await CreateDefaultUsers();
        if (vincent is null)
            throw new NullReferenceException(nameof(vincent));
    }

    //private async Task InitializeForDevelopmentAsync(CancellationToken cancellationToken)
    //{
    //    _logger.Information("Running database initializer...applying development migrations");
    //}

    //private async Task InitializeForTestAsync(CancellationToken cancellationToken)
    //{
    //    _logger.Information("Running database initializer...applying test migrations");
    //}

    //private async Task InitializeForStagingAsync(CancellationToken cancellationToken)
    //{
    //    _logger.Information("Running database initializer...applying staging migrations");
    //}

    //private async Task InitializeForProductionAsync(CancellationToken cancellationToken)
    //{
    //    _logger.Warning("Running database initializer...applying productions migrations");
    //}

    private async Task<ApplicationUser?> CreateDefaultUsers()
    {
        _logger.Information("Running database initializer...creating default users");

        var webmaster = await CreateUser(new ApplicationUser()
        {
            UserName = "webmaster@huybrechts.xyz",
            Email = "webmaster@huybrechts.xyz",
            GivenName = "Webmaster",
            Surname = "Huybrechts",
            EmailConfirmed = true
        });
        if (webmaster is null)
            _logger.Information("Running database initializer...error creating users");

        var sysadmin = await CreateRole(new ApplicationRole()
        {
            Name = ApplicationRole.SystemAdministrator,
            Label = ApplicationRole.SystemAdministrator
        });
        if (sysadmin is null)
            _logger.Information("Running database initializer...error creating roles");

        var user = await CreateRole(new ApplicationRole()
        {
            Name = ApplicationRole.SystemUser,
            Label = ApplicationRole.SystemUser
        });
        if (user is null)
            _logger.Information("Running database initializer...error creating roles");

        if (webmaster is not null && sysadmin is not null)
        {
            if (await _userManager!.IsInRoleAsync(webmaster, sysadmin.Name!))
                return webmaster;

            var result = await _userManager!.AddToRoleAsync(webmaster, sysadmin.Name!);
            if (result.Succeeded)
                return webmaster;

            foreach (var error in result.Errors)
                _logger.Error("Error adding role {role} to user {user}: {errorcode} with {errortext}", sysadmin.Name, webmaster.Email, error.Code, error.Description);
        }

        return webmaster;
    }

    private async Task<ApplicationRole?> CreateRole(ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(() => role);
        ArgumentNullException.ThrowIfNull(() => role.Name);
        var item = await _roleManager!.FindByNameAsync(role.Name!);
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
        var item = await _userManager!.FindByEmailAsync(user.Email!);
        if (item is not null)
            return item;

        var result = await _userManager.CreateAsync(user, _applicationSettings.GetDefaultPassword());
        if (result.Succeeded)
            return await _userManager.FindByEmailAsync(user.Email!);

        foreach (var error in result.Errors)
            _logger.Error("Error creating default user {user}: {errorcode} with {errortext}", user.Email, error.Code, error.Description);

        return null;
    }
}