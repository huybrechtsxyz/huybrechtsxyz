using Huybrechts.Website.Components;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.IO.Compression;

try 
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
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

    Log.Information("Add services to the container");
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    Log.Information("Enabling response compression with brotli and gzip");
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

    Log.Information("Building the application and services");
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Configure the HTTP request pipeline for development");
        //app.UseMigrationsEndPoint(); has already been executed
    }
    else if (app.Environment.IsStaging())
    {
        Log.Information("Configure the HTTP request pipeline for staging");
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }
    else if (app.Environment.IsProduction())
    {
        Log.Information("Configure the HTTP request pipeline for production");
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }
    else
    {
        throw new Exception("Invalid application environment");
    }

    Log.Information("Initializing application services");
    app.UseResponseCompression();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    Log.Information("Mapping and routing razor components");
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Run configured application");
    app.Run();

}
catch (Exception ex)
{
	Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
