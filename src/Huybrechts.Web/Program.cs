using Huybrechts.Infra.Config;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Formatting.Compact;
using System.IO.Compression;

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
    if (ApplicationSettings.IsRunningInContainer())
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
    }
    builder.Services.AddResponseCaching();
    builder.Services.AddResponseCompression(options => {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Fastest;
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.SmallestSize;
    });

    Log.Information("Configuring user interface");
	builder.Services.AddRazorPages().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

	Log.Information("Building the application and services");
    foreach (var service in builder.Services)
        Log.Information(service.ToString());

    Log.Information("Building the application and services");
    var app = builder.Build();

    Log.Information("Initializing application services");
    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=86400"; //+ (int)(60 * 60 * 24);
        }
    });

    Log.Information("Configuring running in containers");
    if (ApplicationSettings.IsRunningInContainer())
    {
        app.UseForwardedHeaders();
    }

    Log.Information("Configuring middleware services");
    app.UseSerilogRequestLogging();
    app.UseResponseCaching();

    Log.Information("Mapping and routing razor components");
	app.MapRazorPages();

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
