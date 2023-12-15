using Huybrechts.Website.Components;
using Huybrechts.Website.Components.Account;
using Huybrechts.Website.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
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

	Log.Information("Configure identity core");
	builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
		.AddEntityFrameworkStores<DatabaseContext>()
		.AddSignInManager()
		.AddDefaultTokenProviders();

	// Database migrations
	Log.Information("Adding database initializer as hosted service");
	builder.Services.AddHostedService<DatabaseInitializer>();

	Log.Information("Add services to the container");
	builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
    builder.Services.AddRazorPages();
    builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();
	builder.Services.AddMudServices();
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

	Log.Information("Mapping and routing razor components");
	app.UseSerilogRequestLogging();
	app.MapRazorPages();
	app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
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