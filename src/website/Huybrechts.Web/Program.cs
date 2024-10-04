using Hangfire;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Serilog;

try
{
    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();
    Log.Information("Starting application");

    Log.Information("Creating application builder");
    /* https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#command-line
	 * provides default configuration for the app in the following order, from highest to lowest priority:
	 * Command-line arguments using the Command-line configuration provider.
	 * Non-prefixed environment variables using the Non-prefixed environment variables configuration provider.
	 * User secrets when the app runs in the Development environment.
	 * appsettings.{Environment}.json using the JSON configuration provider. For example, appsettings.Production.json and appsettings.Development.json.
	 * appsettings.json using the JSON configuration provider.
	 */
    var builder = WebApplication.CreateBuilder(args);
    builder.AddLoggingServices();
    Log.Information("Startup configuration for {environment}", builder.Environment.EnvironmentName);
    builder.Configuration.AddXyzDockerSecrets(builder.Configuration, Log.Logger);
    Log.Information("Startup configuration.............................");
    Log.Information(builder.Configuration.GetDebugView());
    //Log.Information(ApplicationSettings.GetSmtpServerOptions(builder.Configuration).ToLogString());
    Log.Information("Startup configuration.............................");

    Log.Information("Add options to configuration");
    builder.Services.AddSingleton(ApplicationSettings.GetSmtpServerOptions(builder.Configuration));
    builder.Services.AddSingleton(ApplicationSettings.GetPlatformImportOptions(builder.Configuration));
    Log.Information("Connect to the database");
    builder.AddDatabaseServices(Log.Logger);
    Log.Information("Connect to the identity provider");
    builder.AddIdentityServices(Log.Logger);
    Log.Information("Configuring webserver");
    builder.AddWebconfigServices();
    Log.Information("Configuring other services");
    builder.AddConfigurationServices();
    Log.Information("Configuring user interface");
    //builder.Services.AddSingleton<IStringLocalizerFactory, FeatureStringLocalizerFactory>();
    builder.Services.AddSingleton<IResourceNamesCache, ResourceNamesCache>();
    builder.Services.AddLocalization();
    builder.Services.AddAntiforgery();
    builder.Services.AddControllers();
    builder.Services.AddRazorPages()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(opts =>
    {
        opts.IdleTimeout = TimeSpan.FromMinutes(5);
    });

    Log.Information("Building the application with services");
    foreach (var service in builder.Services)
        Log.Debug(service.ToString());
    Log.Debug("Building the application with configuration");
    Log.Debug(builder.Configuration.GetDebugView());

    // Database migrations
    Log.Information("Adding database initializer as hosted service");
    builder.Services.AddHostedService<ApplicationSeedWorker>();
    builder.Services.AddHostedService<FeatureSeedWorker>();

    Log.Information("Building the application and services");
    var app = builder.Build();

    Log.Information("Adding application services");
    app.UseHangfireDashboard();

    Log.Information("Adding middleware services");
    app.AddExceptionMiddleware(Log.Logger);
    app.AddTransportSecurityMiddleware(Log.Logger);
    app.AddRedirectionMiddleware(Log.Logger);
    app.AddStaticFilesMiddleware(Log.Logger);
    app.AddRoutingMiddleware(Log.Logger);
    app.AddCorsMiddleware(Log.Logger);
    app.AddSecurityMiddleware(Log.Logger);
    app.AddSessionMiddleware(Log.Logger);
    app.AddEndpointMiddleware(Log.Logger);

    Log.Information("Run configured application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in application");
}
finally
{
    Log.Information("Application shut down complete");
    Log.CloseAndFlush();
}

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
}
