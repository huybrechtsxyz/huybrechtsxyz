using Huybrechts.Website.Client.Pages;
using Huybrechts.Website.Components;
using Huybrechts.Website.Components.Account;
using Huybrechts.Website.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
	Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
	Log.Information("Starting application");

	Log.Information("Creating application builder");
	var builder = WebApplication.CreateBuilder(args);

	Log.Information("Connecting to database");
	var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection") ??
		throw new InvalidOperationException("Connection string 'DatabaseConnection' not found.");
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(connectionString));
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();

	Log.Information("Building the authentication services");
	builder.Services.AddCascadingAuthenticationState();
	builder.Services.AddScoped<IdentityUserAccessor>();
	builder.Services.AddScoped<IdentityRedirectManager>();
	builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();
	builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	})
	.AddIdentityCookies();

	builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
		.AddEntityFrameworkStores<ApplicationDbContext>()
		.AddSignInManager()
		.AddDefaultTokenProviders();

	// Add services to the container
	Log.Information("Add services to the container");
	builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents()
		.AddInteractiveWebAssemblyComponents();

	builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

	Log.Information("Building the application and services");
	var app = builder.Build();

	// Configure the HTTP request pipeline.
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	if (app.Environment.IsDevelopment())
	{
		Log.Information("Configure the HTTP request pipeline for development");
		app.UseWebAssemblyDebugging();
		app.UseMigrationsEndPoint();
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
	app.UseAuthorization();

	Log.Information("Mapping and routing razor components");
	app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode()
		.AddInteractiveWebAssemblyRenderMode()
		.AddAdditionalAssemblies(typeof(Counter).Assembly);
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
