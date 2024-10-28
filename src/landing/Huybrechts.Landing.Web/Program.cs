using Huybrechts.Shared.Web;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Serilog;

try
{
    Log.Logger = new LoggerConfiguration()
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
    builder.Host.UseSerilog((context, services, configuration) => configuration
           .ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext(),
           writeToProviders: true);
    Log.Information("Startup configuration for {environment}", builder.Environment.EnvironmentName);
    builder.Configuration.AddDockerSecrets(builder.Configuration, Log.Logger);
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
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Configure the HTTP request pipeline for DEVELOPMENT");
        app.UseDeveloperExceptionPage();
    }
    else if (app.Environment.IsTest())
    {
        Log.Information("Configure the HTTP request pipeline for TEST");
        app.UseDeveloperExceptionPage();
        app.UseExceptionHandler("/Error");
    }
    else if (app.Environment.IsStaging())
    {
        Log.Information("Configure the HTTP request pipeline for staging");
        app.UseExceptionHandler("/Error");
    }
    else if (app.Environment.IsProduction())
    {
        Log.Information("Configure the HTTP request pipeline for staging");
        app.UseExceptionHandler("/Error");
    }
    else
        throw new Exception("Invalid application environment for " + app.Environment.EnvironmentName);

    app.AddRedirectionMiddleware(Log.Logger);
    app.AddStaticFilesMiddleware(Log.Logger);
    app.AddLocalizationMiddleware(Log.Logger);
    app.AddRoutingMiddleware(Log.Logger);

    Log.Information("Configure authentication and authorization");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.UseResponseCaching();
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