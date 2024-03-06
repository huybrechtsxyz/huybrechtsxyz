using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Data.Workers;
using Huybrechts.App.Extensions;
using Huybrechts.App.Identity;
using Huybrechts.App.Identity.Entities;
using Huybrechts.App.Identity.Services;
using Huybrechts.Web.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
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
        .Enrich.FromLogContext(),
        //.WriteTo.Console(new RenderedCompactJsonFormatter())
		//.WriteTo.File("logs/website-.txt", rollingInterval: RollingInterval.Day),
		writeToProviders: true);

	CommandLineOptions commandLineOptions = new();
	if (args is not null && args.Length > 0)
	{
		Log.Information($"CommandLine options: {string.Join(" ", args)}");
		int clreturn = commandLineOptions.Interpret(args);
		if (clreturn != 0)
		{
			Log.Error(commandLineOptions.Exception ?? new ApplicationException("Error interpreting the command line options"),
				"Error interpreting command line {args} with {Error}",
				string.Join(" ", args),
				clreturn);
			throw new ApplicationException("Invalid command line");
		}
		Log.Information("Command line for provider: {DatabaseProviderType}", commandLineOptions.DatabaseProviderType);		
	}

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

	Log.Information("Connect to the database");
	ApplicationSettings applicationSettings = new(builder.Configuration);
	DatabaseProviderType connectionType = DatabaseProviderType.None;
	if (commandLineOptions.DatabaseProviderType != DatabaseProviderType.None)
		connectionType = commandLineOptions.DatabaseProviderType;
	else
		connectionType = applicationSettings.GetApplicationConnectionType();
	var connectionString = applicationSettings.GetApplicationConnectionString();
	//Log.Debug("Connecting to {DatabaseProvider} with {DatabaseContext}", connectionType, connectionString);
	switch (connectionType)
	{
		case DatabaseProviderType.SqlLite:
			{
				builder.Services.AddDbContext<ApplicationContext>(options => 
					options.UseSqlite(connectionString, 
					x => x.MigrationsAssembly("Huybrechts.Infra.SqlLite")));
				break;
			}
		case DatabaseProviderType.SqlServer:
			{
				builder.Services.AddDbContext<ApplicationContext>(
					options => options.UseSqlServer(connectionString,
					x => x.MigrationsAssembly("Huybrechts.Infra.SqlServer")));
				break;
			}
		case DatabaseProviderType.PostgreSQL:
			{
				builder.Services.AddDbContext<ApplicationContext>(
					options => options.UseNpgsql(connectionString,
					x => x.MigrationsAssembly("Huybrechts.Infra.PostgreSQL")));
				break;
			}
		default: throw new NotSupportedException("Invalid database context type given for ApplicationDbType");
	}
	ApplicationContext.GlobalDatabaseProvider = connectionType;
	if (builder.Environment.IsDevelopment() || builder.Environment.IsTest())
	{
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();
	}

	Log.Information("Configure authentication");
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
    builder.Services.TryAddScoped<ApplicationUserStore>();
	builder.Services.TryAddScoped<ApplicationUserManager>();
	builder.Services.TryAddScoped<ApplicationSignInManager>();
	builder.Services.TryAddScoped<ApplicationUserClaimsPrincipalFactory>();
	builder.Services.AddSingleton<IEmailSender<ApplicationUser>, AuthenticationSender>();
	builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
		{
			options.SignIn.RequireConfirmedAccount = false;
		})
		.AddEntityFrameworkStores<ApplicationContext>()
		.AddRoleValidator<ApplicationRoleValidator>()
		.AddRoleManager<ApplicationRoleManager>()
		.AddUserStore<ApplicationUserStore>()
		.AddUserManager<ApplicationUserManager>()
		.AddSignInManager<ApplicationSignInManager>()
		.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
		.AddDefaultTokenProviders();

	GoogleClientSecretOptions? google = applicationSettings.GetGoogleLoginClientSecret();
	if (!(google is null || string.IsNullOrEmpty(google.ClientId) || string.IsNullOrEmpty(google.ClientSecret)))
	{
		builder.Services.AddAuthentication().AddGoogle(options =>
		{
			options.ClientId = google.ClientId;
			options.ClientSecret = google.ClientSecret;
			//options.ClaimActions.MapJsonKey("image", "picture"); maps claim name to othername if needed
			options.Scope.Add("profile");
			options.Events.OnCreatingTicket = (context) =>
			{
				var picture = context.User.GetProperty("picture").GetString();
				if (context.Identity is not null && picture is not null)
					context.Identity.AddClaim(new System.Security.Claims.Claim("picture", picture));
				return Task.CompletedTask;
			};
		});
	}

    Log.Information("Add services to container");
	builder.Services.AddTransient<ApplicationTenantManager>();

    Log.Information("Configuring user interface");
	builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
	builder.Services.AddAntiforgery();
	builder.Services.AddControllers();
	builder.Services.AddRazorPages().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
	builder.Services.AddRazorComponents().AddInteractiveServerComponents();

	// Database migrations
	Log.Information("Adding database initializer as hosted service");
	builder.Services.AddHostedService<DatabaseSeedWorker>();

	Log.Information("Building the application with services");
    foreach (var service in builder.Services)
        Log.Debug(service.ToString());

	//Log.Debug("Building the application with configuration");
	//foreach (var config in builder.Configuration.AsEnumerable())
	//	Log.Debug(config.ToString());

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
	else
		app.UseStaticFiles();
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
	app.MapRazorComponents<Huybrechts.Web.Components.App>()
		.AddInteractiveServerRenderMode();
	app.MapAdditionalIdentityEndpoints();

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

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
}