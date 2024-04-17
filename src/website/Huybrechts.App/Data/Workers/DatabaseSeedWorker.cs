using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.App.Data.Workers;

public class DatabaseSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly ILogger _logger = Log.Logger.ForContext<DatabaseSeedWorker>();
	
    public DatabaseSeedWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
	}

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Running database initializer...");
        //var environment = _serviceProvider.GetRequiredService<IWebHostEnvironment>() ??
        //    throw new Exception("The WebHostEnvironment service was not registered as a service");

        using var scope = _serviceProvider.CreateScope();
        var dbcontext = scope.ServiceProvider.GetRequiredService<ApplicationContext>() ??
            throw new Exception("The DatabaseContext service was not registered as a service");

        _logger.Information("Running database initializer...applying database migrations");
        await dbcontext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}