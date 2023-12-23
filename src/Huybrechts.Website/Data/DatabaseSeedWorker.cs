﻿using Huybrechts.Website.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Huybrechts.Website.Data;

public class DatabaseSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;
	private DatabaseContext? _dbcontext = null;
	private UserManager<ApplicationUser> _userManager;
	private RoleManager<ApplicationRole> _roleManager;

	private readonly ILogger _logger = Log.Logger.ForContext<DatabaseSeedWorker>();

	public DatabaseSeedWorker(
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
		_dbcontext = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ??
			throw new Exception("The DatabaseContext service was not registered as a service");

		_userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetRequiredService(typeof(UserManager<ApplicationUser>));
		_roleManager = (RoleManager<ApplicationRole>)scope.ServiceProvider.GetRequiredService(typeof(RoleManager<ApplicationRole>));

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
	}

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task InitializeForAllAsync(CancellationToken cancellationToken)
	{
		/*
		foreach(var roleEnum in Enum.GetValues(typeof(ApplicationRoleEnum)).Cast<ApplicationRoleEnum>())
		{
			if(!await _roleManager.RoleExistsAsync(roleEnum.ToString()))
			{
				await _roleManager.CreateAsync(new ApplicationRole()
				{
					Name = roleEnum.ToString()
				});
			}
		}

		var sysadminUser = new ApplicationUser()
		{
			UserName = "sysadmin",
			Email = "vincent@huybrechts.xyz",
			FirstName = "Vincent",
			LastName = "Huybrechts",
			EmailConfirmed = true
		};
		var user = await _userManager.FindByEmailAsync(sysadminUser.Email);
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
}
