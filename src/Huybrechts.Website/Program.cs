using Huybrechts.Helpers;
using Huybrechts.Website.Components;
using Huybrechts.Website.Components.Account;
using Huybrechts.Website.Data;
using Huybrechts.Website.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

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

	Log.Information("Connect to the database");
	var settingHelper = new SettingHelper(builder.Configuration);
	var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
	builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();

	Log.Information("Configure authentication");
	builder.Services.AddCascadingAuthenticationState();
	builder.Services.AddScoped<IdentityUserAccessor>();
	builder.Services.AddScoped<IdentityRedirectManager>();
	builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

	builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	})
	.AddIdentityCookies();

	var google = settingHelper.GetGoogleAuthentication();
	if (!(google is null || string.IsNullOrEmpty(google.ClientId) || string.IsNullOrEmpty(google.ClientSecret)))
	{
		builder.Services.AddAuthentication().AddGoogle(googleOptions =>
		{
			googleOptions.ClientId = google.ClientId;
			googleOptions.ClientSecret = google.ClientSecret;
		});
	}

    Log.Information("Configure identity core");
	builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
		.AddEntityFrameworkStores<DatabaseContext>()
		.AddSignInManager()
		.AddDefaultTokenProviders();

	// Database migrations
	Log.Information("Adding database initializer as hosted service");
	builder.Services.AddHostedService<DatabaseInitializer>();
    
    Log.Information("Add services to the container");
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, AuthenticationSender>();
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddControllers();
	builder.Services.AddRazorPages()
		.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
		//.AddDataAnnotationsLocalization(); Not supported by blazor at this moment
	builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();
	
    builder.Services.Configure<CookiePolicyOptions>(options =>
	{
		// This lambda determines whether user consent for non-essential cookies is needed for a given request.
		options.CheckConsentNeeded = context => true;
		options.MinimumSameSitePolicy = SameSiteMode.None;
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
	app.UseHttpsRedirection();
	app.UseStaticFiles();
	app.UseCookiePolicy();
	app.UseAntiforgery();

    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        SupportedCultures = SettingHelper.SupportedCultures,
        SupportedUICultures = SettingHelper.SupportedCultures,
        DefaultRequestCulture = new RequestCulture(SettingHelper.SupportedCultures[0])
    });

    Log.Information("Mapping and routing razor components");
	app.UseSerilogRequestLogging();
    app.MapControllers();
    app.MapRazorPages();
	app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();
	app.MapAdditionalIdentityEndpoints();

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