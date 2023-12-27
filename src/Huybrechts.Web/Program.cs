using Huybrechts.Infra.Config;
using Huybrechts.Infra.Data;
using Huybrechts.Infra.Services;
using Huybrechts.Web.Components;
using Huybrechts.Web.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
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
    ApplicationSettings applicationSettings = new(builder.Configuration);

    Log.Information("Configuring webserver");
    builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection("Kestrel"));
    if (applicationSettings.IsRunningInContainer())
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
    }
    builder.Services.AddResponseCaching();

    Log.Information("Connect to the database");
    DatabaseContextType connectionType = applicationSettings.GetApplicationDatabaseType();
    var connectionString = applicationSettings.GetApplicationDatabaseConnectionString();
    switch (connectionType)
    {
        case DatabaseContextType.SqlServer:
            {
                builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));
                break;
            }
        case DatabaseContextType.PostgreSQL:
            {
                builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString));
                break;
            }
        case DatabaseContextType.None:
            {
                builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connectionString));
                break;
            }
    }
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

    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<DatabaseContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, AuthenticationSender>();

    AuthenticationSettings? google = applicationSettings.GetGoogleAuthentication();
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

    Log.Information("Add cookie policy options");
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
    });

    Log.Information("Add services to the container");
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
    builder.Services.AddAntiforgery();
    builder.Services.AddControllers();
    builder.Services.AddRazorPages()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

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

    //builder.Services.AddFastEndpoints();
    //builder.Services.SwaggerDocument(o =>
    //{
    //    o.ShortSchemaNames = true;
    //});

    // Database migrations
    Log.Information("Adding database initializer as hosted service");
    builder.Services.AddHostedService<DatabaseSeedWorker>();

    Log.Information("Building the application and services");
    var app = builder.Build(); ;

    // Configure the HTTP request pipeline.
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Configure the HTTP request pipeline for development");
        app.UseDeveloperExceptionPage();
        //app.UseWebAssemblyDebugging();
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
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=86400"; //+ (int)(60 * 60 * 24);
        }
    });
    app.UseCookiePolicy();
    
    if (applicationSettings.IsRunningInContainer())
    {
        app.UseForwardedHeaders();
    }

    Log.Information("Initializing application localization");
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        SupportedCultures = ApplicationSettings.GetSupportedCultures(),
        SupportedUICultures = ApplicationSettings.GetSupportedCultures(),
        DefaultRequestCulture = new RequestCulture(ApplicationSettings.GetDefaultCulture())
    });

    Log.Information("Mapping and routing razor components");
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAntiforgery();
    //app.UseFastEndpoints();
    //app.UseSwaggerGen(); // FastEndpoints middleware
    app.UseResponseCaching();
    app.UseAuthorization();
    app.MapControllers();
    app.MapRazorPages();
    app.MapRazorComponents<Huybrechts.Web.Components.App>()
        .AddInteractiveServerRenderMode();
    app.MapAdditionalIdentityEndpoints();
    //app.MapGet("/", () => "Hello World!");

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