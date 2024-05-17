using Huybrechts.App.Config;
using Huybrechts.App.Identity;
using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;

namespace Huybrechts.App.Data.Workers;

public class DatabaseSeedWorker : IHostedService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;
	private readonly ILogger _logger = Log.Logger.ForContext<DatabaseSeedWorker>();

	private ApplicationContext? _dbcontext = null;
	private ApplicationUserManager? _userManager = null;
	private ApplicationRoleManager? _roleManager = null;

	public DatabaseSeedWorker(IServiceProvider serviceProvider, IConfiguration configuration)
	{
		_serviceProvider = serviceProvider;
		_configuration = configuration;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.Information("Running database initializer...");
		//var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
		//    throw new Exception("The WebHostEnvironment service was not registered as a service");

		using var scope = _serviceProvider.CreateScope();
		_dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationContext>() ??
			throw new Exception("The DatabaseContext service was not registered as a service");

		_userManager = (ApplicationUserManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationUserManager));
		_roleManager = (ApplicationRoleManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationRoleManager));

        _logger.Information("Running database initializer...applying database migrations");
        if (HealthStatus.Unhealthy == await MigrateAsync(5, 5, new CancellationToken()))
        {
            Log.Fatal("Unable to connect to or migrate the database");
            throw new ApplicationException("Unable to reach database...ending program.");
        }

		await InitializeForAllAsync(); //cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task<HealthStatus> MigrateAsync(int maxRetries, int initialDelaySeconds, CancellationToken cancellationToken)
	{
		HealthStatus healthCheckResult = HealthStatus.Unhealthy;
		int retryCount = 0;

		if (_dbcontext is null)
			return healthCheckResult;

		while (retryCount < maxRetries)
		{
			try
			{
				await _dbcontext.Database.MigrateAsync(cancellationToken);
				initialDelaySeconds = 0;
				return HealthStatus.Healthy;
			}
			catch (Exception ex)
			{
				Log.Information(ex, "Unable to migrate database, retry {AppDataRetryCount} of {AppDataMaxRetries}", retryCount, maxRetries);
			}
			finally
			{
				retryCount++;
				int delay = (int)(initialDelaySeconds * 1000 * Math.Pow(2, retryCount));
				await Task.Delay(delay, cancellationToken);
			}
		}

		return healthCheckResult;
	}

    private async Task InitializeForAllAsync() //CancellationToken cancellationToken)
	{
		var vincent = await CreateDefaultUsers();
		if (vincent is null)
			throw new NullReferenceException(nameof(vincent));
	}

	private async Task<ApplicationUser?> CreateDefaultUsers()
	{
		_logger.Information("Running database initializer...creating default users");

		var webmaster = await CreateAdministrator();
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

		return null;
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

	private async Task<ApplicationUser?> CreateAdministrator()
	{
		var administrator = new ApplicationUser()
		{
			UserName = _configuration.GetValue<string>(ApplicationSettings.ENV_APP_HOST_EMAIL) ?? string.Empty,
			Email = _configuration.GetValue<string>(ApplicationSettings.ENV_APP_HOST_EMAIL) ?? string.Empty,
			GivenName = _configuration.GetValue<string>(ApplicationSettings.ENV_APP_HOST_USERNAME) ?? string.Empty,
			Surname = "",
			EmailConfirmed = true
		};

		if (string.IsNullOrEmpty(administrator.Email))
			throw new InvalidOperationException("Running database initializer...creating administrator with invalid values");
		if (string.IsNullOrEmpty(administrator.UserName))
			throw new InvalidOperationException("Running database initializer...creating administrator with invalid values");
		
		var item = await _userManager!.FindByEmailAsync(administrator.Email!);
		if (item is not null)
			return item;

		var result = await _userManager.CreateAsync(administrator, _configuration.GetValue<string>(ApplicationSettings.ENV_APP_HOST_PASSWORD) ?? string.Empty);
		if (result.Succeeded)
			return await _userManager.FindByEmailAsync(administrator.Email!);

		foreach (var error in result.Errors)
			_logger.Error("Error creating default user {user}: {errorcode} with {errortext}", administrator.Email, error.Code, error.Description);

		return null;
	}
}