using Huybrechts.Infra.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Tls;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Huybrechts.Infra.Data;

public class AdministrationSeedWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
	private readonly IConfiguration _configuration;
	private AdministrationContext? _dbcontext = null;
	private UserManager<ApplicationUser>? _userManager;
	private RoleManager<ApplicationRole>? _roleManager;

	private readonly ILogger _logger = Log.Logger.ForContext<AdministrationSeedWorker>();

	public AdministrationSeedWorker(
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
        _dbcontext = scope.ServiceProvider.GetRequiredService<AdministrationContext>() ??
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

        _logger.Information("Running database initializer...loading tenants");
        var tenantCollection = (TenantContextCollection)scope.ServiceProvider.GetRequiredService(typeof(ITenantContextCollection));
        TenantContextFactory tenantFactory = new();
        tenantFactory.BuildTenantCollection(_dbcontext, tenantCollection);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task InitializeForAllAsync(CancellationToken cancellationToken)
	{
        _logger.Information("Running database initializer...applying migrations");

        await CreateDefaultApplicationRoles();

        /*var sysadminUser = new ApplicationUser()
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

	private async Task CreateDefaultApplicationRoles()
	{
		if (_roleManager is not null)
		{
			var defaultRoles = ApplicationRole.GetDefaultRoles();
			foreach (var defaultRole in defaultRoles)
			{
				if (!await _roleManager.RoleExistsAsync(defaultRole.Name!))
				{
					await _roleManager.CreateAsync(new ApplicationRole()
					{
						Name = defaultRole.Name!
					});
				}
			}
		}
	}
}
