using Huybrechts.Infra.Config;
using Huybrechts.Infra.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
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
        .WriteTo.Console(new RenderedCompactJsonFormatter())
		.WriteTo.File("logs/website-.txt", rollingInterval: RollingInterval.Day),
		writeToProviders: true);

	if (ApplicationSettings.IsRunningInContainer())
		builder.Configuration.AddDockerSecrets("/run/secrets", ":", null);

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
		builder.Services.Configure<CookiePolicyOptions>(options =>
		{
			// This lambda determines whether user consent for non-essential cookies is needed for a given request.
			options.CheckConsentNeeded = context => true;
			options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
		});

		Log.Information("Configuring user interface");
		builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
		builder.Services.AddAntiforgery();
		builder.Services.AddControllers();
		builder.Services.AddRazorPages().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

		Log.Information("Building the application and services");
    foreach (var service in builder.Services)
        Log.Information(service.ToString());

    Log.Information("Building the application and services");
    var app = builder.Build();

		// Configure the HTTP request pipeline.
		// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
		if (app.Environment.IsDevelopment())
		{
			Log.Information("Configure the HTTP request pipeline for development");
			app.UseDeveloperExceptionPage();
			//app.UseWebAssemblyDebugging();
		}
		else if (app.Environment.IsStaging())
		{
			Log.Information("Configure the HTTP request pipeline for staging");
			app.UseExceptionHandler("/Error", createScopeForErrors: true);
			if (!ApplicationSettings.IsRunningInContainer())
					app.UseHsts();
		}
		else if (app.Environment.IsProduction())
		{
			Log.Information("Configure the HTTP request pipeline for production");
			app.UseExceptionHandler("/Error", createScopeForErrors: true);
			if (!ApplicationSettings.IsRunningInContainer())
					app.UseHsts();
		}
		else
		{
			throw new Exception("Invalid application environment");
		}

		Log.Information("Initializing application services");
		app.UseResponseCompression();
		if (!ApplicationSettings.IsRunningInContainer())
				app.UseHttpsRedirection();
		app.UseStaticFiles(new StaticFileOptions
		{
			OnPrepareResponse = ctx =>
			{
				ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=86400"; //+ (int)(60 * 60 * 24);
			}
		});
		app.UseRequestLocalization(new RequestLocalizationOptions
		{
			SupportedCultures = ApplicationSettings.GetSupportedCultures(),
			SupportedUICultures = ApplicationSettings.GetSupportedCultures(),
			DefaultRequestCulture = new RequestCulture(ApplicationSettings.GetDefaultCulture())
		});
		app.UseCookiePolicy();

		Log.Information("Configuring running in containers");
		if (ApplicationSettings.IsRunningInContainer())
		{
				app.UseForwardedHeaders();
		}

		Log.Information("Configuring middleware services");
		app.UseSerilogRequestLogging();
		app.UseRouting();
		app.UseAntiforgery();
		app.UseResponseCaching();

		Log.Information("Mapping and routing razor components");
		app.MapControllers();
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
