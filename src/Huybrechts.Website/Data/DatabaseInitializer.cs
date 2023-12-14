using Huybrechts.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Diagnostics.Eventing.Reader;

namespace Huybrechts.Website.Data;


public class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;
	private readonly ILogger _logger;

	private DatabaseContext? _dbcontext = null;

	public DatabaseInitializer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger logger)
    {
		_logger = logger; 
		_configuration = configuration;
		_serviceProvider = serviceProvider;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Running database initializer...");
		var configHelper = new ConfigurationHelper(_configuration);
		var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ?? 
			throw new Exception("The WebHostEnvironment service was not registered as a service");
		
		using var scope = _serviceProvider.CreateScope();
		_dbcontext = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ??
			throw new Exception("The DatabaseContext service was not registered as a service");

		if (environment.IsDevelopment() && configHelper.IsResetEnvironment)
		{
			_logger.LogWarning("Running database initializer...deleting existing database");
			await _dbcontext.Database.EnsureDeletedAsync(cancellationToken);
		}

		_logger.LogInformation("Running database initializer...applying database migrations");
		await _dbcontext.Database.MigrateAsync(cancellationToken);

		if (environment.IsDevelopment())
			await InitializeForDevelopmentAsync(cancellationToken);

		else if (environment.IsStaging())
			await InitializeForStagingAsync(cancellationToken);

        else if (environment.IsProduction())
			await InitializeForProductionAsync(cancellationToken);
	}

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task InitializeForDevelopmentAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Running database initializer...applying development migrations");
	}

	private async Task InitializeForStagingAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Running database initializer...applying staging migrations");
	}

	private async Task InitializeForProductionAsync(CancellationToken cancellationToken)
	{
		_logger.LogWarning("Running database initializer...applying productions migrations");
	}
}
