using Huybrechts.Shared.App.Config;
using Huybrechts.Shared.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Serilog;

try
{
    /* https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#command-line
	 * provides default configuration for the app in the following order, from highest to lowest priority:
	 * Command-line arguments using the Command-line configuration provider.
	 * Non-prefixed environment variables using the Non-prefixed environment variables configuration provider.
	 * User secrets when the app runs in the Development environment.
	 * appsettings.{Environment}.json using the JSON configuration provider. For example, appsettings.Production.json and appsettings.Development.json.
	 * appsettings.json using the JSON configuration provider.
	 */
    Log.Logger = Huybrechts.Shared.Web.WebHostExtensions.AddBootstrapLogger();
    Log.Information("Starting application");

    Log.Information("Creating application builder");
    var builder = WebApplication.CreateBuilder(args);
    builder.AddLoggingServices();
    Log.Information("Startup configuration for {environment}", builder.Environment.EnvironmentName);
    builder.Configuration.AddDockerSecrets(builder.Configuration, Log.Logger);
    builder.Configuration.AddConsul("landing", Log.Logger);
    builder.AddWebconfigServices(Log.Logger);
    builder.AddCookiePolicies(Log.Logger);

    Log.Information("Configuring other services");
    builder.Services.AddSingleton<IResourceNamesCache, ResourceNamesCache>();
    builder.Services.AddLocalization();
    builder.Services.AddAntiforgery();
    builder.Services.AddRazorPages()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();

    Log.Information("Building the application and services");
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    Log.Information("Configure the HTTP request pipeline");
    app.UseExceptionMiddleware(false, Log.Logger);
    app.UseHstsMiddleware(Log.Logger);
    app.UseRedirecionMiddleware(Log.Logger);
    app.UseStaticFilesMiddleware(Log.Logger);
    app.UseLocalizationMiddleware(Log.Logger);
    app.UseRoutingMiddleware(Log.Logger);
    app.UseCorsMiddleware(Log.Logger);
    app.UseClientSecurityMiddleware(Log.Logger);
    app.UseCustomMiddleware(Log.Logger);
    app.UseMiniProfiler();

    Log.Information("Mapping and routing razor components");
    app.MapRazorPages();

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