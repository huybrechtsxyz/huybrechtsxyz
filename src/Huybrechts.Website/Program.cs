using Huybrechts.Website.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    Log.Information("Creating application builder");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    Log.Information("Add services to the container");
    builder.Services.AddRazorComponents()
    .   AddInteractiveServerComponents();

    Log.Information("Building the application and services");
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Configure the HTTP request pipeline for development");
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
    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    //app.UseSerilogRequestLogging();
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