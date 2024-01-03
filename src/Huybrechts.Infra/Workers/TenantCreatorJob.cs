using Huybrechts.Infra.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using System.Threading;
using ILogger = Serilog.ILogger;

namespace Huybrechts.Infra.Workers;

public class TenantCreatorJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    private readonly ILogger _logger = Log.Logger.ForContext<TenantCreatorJob>();

    public TenantCreatorJob(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
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

        await InitializeForAllAsync();

        if (environment.IsDevelopment())
            await InitializeForDevelopmentAsync();

        else if (environment.IsStaging())
            await InitializeForStagingAsync();

        else if (environment.IsProduction())
            await InitializeForProductionAsync();
    }

    private async Task InitializeForAllAsync()
    {
        _logger.Information("Running tenant initializer...applying migrations");
    }

    private async Task InitializeForDevelopmentAsync()
    {
        _logger.Information("Running tenant initializer...applying development migrations");
    }

    private async Task InitializeForStagingAsync()
    {
        _logger.Information("Running tenant initializer...applying staging migrations");
    }

    private async Task InitializeForProductionAsync()
    {
        _logger.Warning("Running tenant initializer...applying productions migrations");
    }
}
