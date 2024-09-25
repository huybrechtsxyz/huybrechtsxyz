using Huybrechts.App.Application;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.App.Data;

public class ApplicationSeedWorker : IHostedService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;
	private readonly ILogger _logger = Log.Logger.ForContext<ApplicationSeedWorker>();

	private ApplicationContext _dbcontext = null!;
	private ApplicationUserManager _userManager = null!;
	private ApplicationRoleManager _roleManager = null!;

	public ApplicationSeedWorker(IServiceProvider serviceProvider, IConfiguration configuration)
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

		_userManager = (ApplicationUserManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationUserManager)) ??
            throw new Exception("The ApplicationUserManager service was not registered as a service");
        _roleManager = (ApplicationRoleManager)scope.ServiceProvider.GetRequiredService(typeof(ApplicationRoleManager)) ??
            throw new Exception("The ApplicationRoleManager service was not registered as a service");

        _logger.Information("Running database initializer...applying database migrations");
        if (HealthStatus.Unhealthy == await MigrateAsync(5, 5, new CancellationToken()))
        {
            Log.Fatal("Unable to connect to or migrate the database");
            throw new ApplicationException("Unable to reach database...ending program.");
        }

		await InitializeForAllAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task<HealthStatus> MigrateAsync(int maxRetries, int initialDelaySeconds, CancellationToken cancellationToken)
	{
		HealthStatus healthCheckResult = HealthStatus.Unhealthy;
		int retryCount = 1;

		if (_dbcontext is null)
			return healthCheckResult;

		while (retryCount <= maxRetries)
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

	private async Task InitializeForAllAsync(CancellationToken cancellationToken = default)
	{
		await CreateDefaultUsersAndRoles();
	}

	private async Task CreateDefaultUsersAndRoles()
	{
		_logger.Information("Running database initializer...building default roles and users");
		List<ApplicationRole> defaultRoles = ApplicationRole.GetDefaultSystemRoles();
		List<ApplicationUser> defaultUsers = GetDefaultUsers();

		_logger.Information("Running database initializer...creating default roles");
		foreach (var item in defaultRoles)
		{
			var newRole = await CreateRole(item);
			if (newRole is null)
				_logger.Error($"Running database initializer...error creating default role {item.Name}");
		}

		ApplicationRole systemAdministrator = await _roleManager.FindByNameAsync(ApplicationRole.GetRoleName(ApplicationSystemRole.Administrator)) ??
			throw new Exception("The System Administrator role was not created properly");

		_logger.Information("Running database initializer...creating default users");
		foreach (var item in defaultUsers)
		{
			var _ = await CreateUser(item, systemAdministrator!) ??
				throw new InvalidOperationException("Running database initializer...unable to create administrator");
		}
	}

	private List<ApplicationUser> GetDefaultUsers()
	{
		return [
			new()
			{
				UserName = ApplicationSettings.GetApplicationHostUsername(_configuration),
				Email = ApplicationSettings.GetApplicationHostEmail(_configuration),
				GivenName = ApplicationSettings.GetApplicationHostUsername(_configuration),
				Surname = ApplicationSettings.GetApplicationHostUsername(_configuration),
				EmailConfirmed = true
			}
		];
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

	private async Task<ApplicationUser?> CreateUser(ApplicationUser user, ApplicationRole role)
	{
		ArgumentNullException.ThrowIfNull(() => user);
		ArgumentNullException.ThrowIfNull(() => user.UserName);
		ArgumentNullException.ThrowIfNull(() => user.Email);
		ArgumentNullException.ThrowIfNull(() => role);
		ArgumentNullException.ThrowIfNull(() => role.NormalizedName);

		var item = await _userManager.FindByEmailAsync(user.Email!);
		if (item is null)
		{
			var results = await _userManager.CreateAsync(user, ApplicationSettings.GetApplicationHostPassword(_configuration));
			if (!results.Succeeded)
			{
				foreach (var error in results.Errors)
					_logger.Error("Error creating default user {user}: {errorcode} with {errortext}", user.Email, error.Code, error.Description);
				return null;
			}

			item = await _userManager.FindByEmailAsync(user.Email!);
			if (item is null || string.IsNullOrEmpty(item.UserName) || string.IsNullOrEmpty(item.Email))
				throw new InvalidOperationException($"Running database initializer...creating user with invalid username or email");
		}

		if (await _userManager.IsInRoleAsync(item, role.NormalizedName!))
            return item;

		var result = await _userManager.AddToRoleAsync(item, role.NormalizedName!);
		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
				_logger.Error("Error adding role {role} to user {user}: {errorcode} with {errortext}", item.UserName, item.Email, error.Code, error.Description);
		}

		return item;
	}
}