using Huybrechts.App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.App.Features.Platform;

public class SetupSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger = Log.Logger.ForContext<FeatureContext>();

    private FeatureContext _dbcontext = null!;

    public SetupSeedWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running setup initializer...");
        //var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
        //    throw new Exception("The WebHostEnvironment service was not registered as a service");

        using var scope = _serviceProvider.CreateScope();
        _dbcontext = scope.ServiceProvider.GetRequiredService<FeatureContext>() ??
            throw new Exception("The FeatureContext service was not registered as a service");

        _logger.Information("Running setup initializer...applying database migrations");
        if (HealthStatus.Unhealthy == await MigrateAsync(5, 5, new CancellationToken()))
        {
            Log.Fatal("Unable to connect to or migrate the setup database");
            throw new ApplicationException("Unable to reach setup database...ending program.");
        }

        //await InitializeForAllAsync(cancellationToken);
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

    //private async Task InitializeForAllAsync(CancellationToken cancellationToken)
    //{

    //}
}