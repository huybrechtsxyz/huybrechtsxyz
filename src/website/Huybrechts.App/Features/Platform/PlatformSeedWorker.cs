using Huybrechts.App.Application;
using Huybrechts.App.Config;
using Huybrechts.Core.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.App.Features.Platform;

public class PlatformSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    //private readonly IConfiguration _configuration;
    private readonly ILogger _logger = Log.Logger.ForContext<PlatformSeedWorker>();

    private PlatformContext _dbcontext = null!;

    public PlatformSeedWorker(IServiceProvider serviceProvider) //, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        //_configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running platform initializer...");
        //var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
        //    throw new Exception("The WebHostEnvironment service was not registered as a service");

        using var scope = _serviceProvider.CreateScope();
        _dbcontext = scope.ServiceProvider.GetRequiredService<PlatformContext>() ??
            throw new Exception("The PlatformContext service was not registered as a service");

        _logger.Information("Running platform initializer...applying database migrations");
        if (HealthStatus.Unhealthy == await MigrateAsync(5, 5, new CancellationToken()))
        {
            Log.Fatal("Unable to connect to or migrate the platform database");
            throw new ApplicationException("Unable to reach platform database...ending program.");
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