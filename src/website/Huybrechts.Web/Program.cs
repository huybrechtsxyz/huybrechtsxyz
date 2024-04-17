using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Data.Workers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO.Compression;
using System.Threading;

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
	builder.Host.UseSerilog((context, services, configuration) => configuration
		.ReadFrom.Configuration(context.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext(),
		writeToProviders: true);

	if (ApplicationSettings.IsRunningInContainer())
		builder.Configuration.AddDockerSecrets("/run/secrets", ":", null);

	Log.Information("Configuring webserver");
	builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection("Kestrel"));
	if (builder.Environment.IsProduction())
	{
		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name: "App-Access-Control-Allow-Origin",
				policy =>
				{
					policy.WithOrigins("https://huybrechts.xyz");
				}
			);
		});
	}
	else if (builder.Environment.IsStaging())
	{
		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name: "App-Access-Control-Allow-Origin",
				policy =>
				{
					policy.WithOrigins("https://staging.huybrechts.xyz");
				}
			);
		});
	}
	else if (builder.Environment.IsTest())
	{
		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name: "App-Access-Control-Allow-Origin",
				policy =>
				{
					policy.WithOrigins("https://test.huybrechts.xyz");
				}
			);
		});
	}
	else if (builder.Environment.IsDevelopment())
	{
		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name: "App-Access-Control-Allow-Origin",
				policy =>
				{
					policy.WithOrigins("http://localhost", "https://localhost", "https://develop.huybrechts.xyz");
				}
			);
		});
	}
	else
	{
		throw new Exception("Invalid application environment");
	}

	Log.Information("Read options from configuration");

	Log.Information("Connect to the database");
	var connectionString = ApplicationSettings.GetApplicationContextConnectionString(builder.Configuration);
	var contextProviderType = ApplicationSettings.GetContextProvider(connectionString);
	builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connectionString, x => x.MigrationsAssembly("Huybrechts.Infra.Sqlite")));
	if (builder.Environment.IsDevelopment() || builder.Environment.IsTest())
	{
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();
	}

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
	builder.Services.AddRazorComponents().AddInteractiveServerComponents();

	Log.Information("Building the application with services");
	foreach (var service in builder.Services)
		Log.Debug(service.ToString());

	Log.Debug("Building the application with configuration");
	Log.Debug(builder.Configuration.GetDebugView());

	// Database migrations
	Log.Information("Adding database initializer as hosted service");
	builder.Services.AddHostedService<DatabaseSeedWorker>();

	Log.Information("Building the application and services");
	var app = builder.Build();

	// Configure the HTTP request pipeline.
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	if (app.Environment.IsDevelopment() || app.Environment.IsTest())
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
	if (app.Environment.IsStaging() || app.Environment.IsProduction())
	{
		app.UseResponseCompression();
		if (!ApplicationSettings.IsRunningInContainer())
			app.UseHttpsRedirection();
		app.UseStaticFiles(new StaticFileOptions()
		{
			OnPrepareResponse = ctx =>
			{
				ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=86400"; //+ (int)(60 * 60 * 24);
			}
		});
	}
	else app.UseStaticFiles();
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
	Log.Information("Using authentication and authorization");
	app.UseSerilogRequestLogging();
	app.UseRouting();
	app.UseAuthentication();
	app.UseAuthorization();
	app.UseAntiforgery();
	app.UseResponseCaching();

	Log.Information("Mapping and routing razor components");
	app.MapControllers();
	app.MapRazorPages();
	app.Map("/", x => { Console.WriteLine("Hello world"); });

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