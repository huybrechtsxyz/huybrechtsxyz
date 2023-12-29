using Huybrechts.Infra.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Huybrechts.Infra.Data;

public class TenantSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;

	private readonly ILogger _logger = Log.Logger.ForContext<TenantSeedWorker>();

	public TenantSeedWorker(
		IServiceProvider serviceProvider,
		IConfiguration configuration)
    {
		_configuration = configuration;
		_serviceProvider = serviceProvider;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.Information("Running tenant initializer...");
        ApplicationSettings applicationSettings = new(_configuration);
		var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ?? 
			throw new Exception("The WebHostEnvironment service was not registered as a service");
		
		if (environment.IsDevelopment() && applicationSettings.DoResetEnvironment())
		{
			_logger.Warning("Running tenant initializer...deleting existing database");
			//await _dbcontext.Database.EnsureDeletedAsync(cancellationToken);
		}

		_logger.Information("Running tenant initializer...applying database migrations");
		//await _dbcontext.Database.MigrateAsync(cancellationToken);

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
        _logger.Information("Running tenant initializer...applying migrations");
    }
	
	private async Task InitializeForDevelopmentAsync(CancellationToken cancellationToken)
	{
		_logger.Information("Running tenant initializer...applying development migrations");
	}

	private async Task InitializeForStagingAsync(CancellationToken cancellationToken)
	{
		_logger.Information("Running tenant initializer...applying staging migrations");
	}

	private async Task InitializeForProductionAsync(CancellationToken cancellationToken)
	{
		_logger.Warning("Running tenant initializer...applying productions migrations");
	}
}
