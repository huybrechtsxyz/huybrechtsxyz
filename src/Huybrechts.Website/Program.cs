using Huybrechts.Website.Components;
using Huybrechts.Website.Components.Account;
using Huybrechts.Website.Data;
using Huybrechts.Website.Helpers;
using Huybrechts.Website.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

    Log.Information("Connect to the database");
    ApplicationSettings applicationSettings = new(builder.Configuration);
    var connectionString = applicationSettings.GetApplicationDatabaseConnectionString();
    builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Database migrations
    Log.Information("Adding database initializer as hosted service");
    builder.Services.AddHostedService<DatabaseSeedWorker>();

	Log.Information("Configure authentication");
	//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	//    .AddEntityFrameworkStores<DatabaseContext>();
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

	builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<DatabaseContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, AuthenticationSender>();

    var google = applicationSettings.GetGoogleAuthentication();
    if (!(google is null || string.IsNullOrEmpty(google.ClientId) || string.IsNullOrEmpty(google.ClientSecret)))
    {
        builder.Services.AddAuthentication().AddGoogle(options =>
        {
            options.ClientId = google.ClientId;
            options.ClientSecret = google.ClientSecret;
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

    Log.Information("Add services to the container");
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");    
    builder.Services.AddControllers();
    builder.Services.AddRazorPages()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    Log.Information("Add cookie policy options");
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

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
    app.UseCookiePolicy();
    app.UseAntiforgery();

    Log.Information("Initializing application localization");
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        SupportedCultures = ApplicationSettings.GetSupportedCultures(),
        SupportedUICultures = ApplicationSettings.GetSupportedCultures(),
        DefaultRequestCulture = new RequestCulture(ApplicationSettings.GetDefaultCulture())
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
