using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Formatting.Compact;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateBootstrapLogger();
    Log.Information("Starting application");

    Log.Information("Creating application builder");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new RenderedCompactJsonFormatter()),
        writeToProviders: true);

    Log.Information("Configuring webserver");
    builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection("Kestrel"));

    Log.Information("Building the application and services");
    foreach (var service in builder.Services)
        Log.Information(service.ToString());

    Log.Information("Building the application and services");
    var app = builder.Build();

    Log.Information("Mapping and routing razor components");
    app.MapGet("/", () => "Hello World!");

    Log.Information("Run configured application");
    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in application");
}
finally
{
    Log.Information("Application shut down complete");
    Log.CloseAndFlush();
}
