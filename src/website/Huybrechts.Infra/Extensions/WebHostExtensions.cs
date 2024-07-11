using Hangfire;
using Hangfire.SQLite;
using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Huybrechts.Infra.Config;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO.Compression;

namespace Huybrechts.Infra.Extensions;

public static class WebHostExtensions
{
    public static bool IsTest(this IWebHostEnvironment env) => env.EnvironmentName == "Test";

    public static WebApplicationBuilder AddXyzSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext(),
            writeToProviders: true);
        return builder;
    }

    public static IConfigurationBuilder AddXyzDockerSecrets(
            this IConfigurationBuilder builder,
            string secretsDirectoryPath,
            string colonPlaceholder,
            ICollection<string>? allowedPrefixes, 
            ILogger log)
    {
        if (!EnvironmentSettings.IsRunningInContainer())
            return builder;

        log.Information("Running application in container");
        log.Information("Reading configuration for docker secrets");

        return builder.Add(new DockerSecretsConfigurationsSource(
            secretsDirectoryPath,
            colonPlaceholder,
            allowedPrefixes));
    }

    public static WebApplicationBuilder AddXyzWebconfig(this WebApplicationBuilder builder)
    {
		builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection("Kestrel"));

		string baseUri = EnvironmentSettings.GetApplicationHostUrl(builder.Configuration);
        if (!string.IsNullOrEmpty(baseUri))
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "App-Access-Control-Allow-Origin",
                    policy =>
                    {
                        policy.WithOrigins(baseUri);
                    }
                );
            });
		}

		if (EnvironmentSettings.IsRunningInContainer())
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        builder.Services.AddResponseCaching();
        builder.Services.AddResponseCompression(options =>
        {
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

        return builder;
    }

    public static WebApplicationBuilder AddXyzDatabase(this WebApplicationBuilder builder, ILogger log)
    {
        //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        //builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(connectionString));
        //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        var connectionString = EnvironmentSettings.GetContextConnectionString(builder.Configuration);
        var contextProviderType = EnvironmentSettings.GetContextProvider(connectionString);

        log.Information($"Connect to the {contextProviderType} database: {connectionString}");
        switch (contextProviderType)
        {
            case ContextProviderType.Sqlite:
                {
                    builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connectionString, x => x.MigrationsAssembly("Huybrechts.Infra.Sqlite")));
                    break;
                }
            case ContextProviderType.SqlServer:
                {
                    builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString, x => x.MigrationsAssembly("Huybrechts.Infra.SqlServer")));
                    break;
                }
            default: throw new ArgumentException($"Unsupported or invalid connection string format: {connectionString}.");
        }

        if (builder.Environment.IsDevelopment() || builder.Environment.IsTest())
        {
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        }

        log.Information($"Connect hangfire to the database");
        switch (contextProviderType)
        {
            case ContextProviderType.Sqlite:
                {
                    builder.Services.AddHangfire(x => x
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSQLiteStorage(connectionString));
                    break;
                }
            case ContextProviderType.SqlServer:
                {
                    builder.Services.AddHangfire(x => x
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSqlServerStorage(connectionString));
                    break;
                }
            default: throw new ArgumentException($"Unsupported or invalid connection for hangfire.");
        }

        builder.Services.AddHangfireServer(x =>
            x.SchedulePollingInterval = TimeSpan.FromSeconds(30)
            );
        return builder;
    }

    public static WebApplicationBuilder AddXyzIdentity(this WebApplicationBuilder builder, ILogger log)
    {
        log.Information("Configure authentication services");
        builder.Services.TryAddScoped<ApplicationUserStore>();
        builder.Services.TryAddScoped<ApplicationUserManager>();
        builder.Services.TryAddScoped<ApplicationRoleStore>();
        builder.Services.TryAddScoped<ApplicationRoleManager>();
        builder.Services.TryAddScoped<ApplicationSignInManager>();
        builder.Services.TryAddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, AuthenticationSender>();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthenticationMailer>();

		log.Information("Configure authentication identity");
        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = (!builder.Environment.IsDevelopment());
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<ApplicationContext>()
        .AddRoleManager<ApplicationRoleManager>()
        .AddUserStore<ApplicationUserStore>()
        .AddUserManager<ApplicationUserManager>()
        .AddSignInManager<ApplicationSignInManager>()
        .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

        builder.Services.TryAddScoped<ApplicationTenantManager>();

        log.Information("Configure authentication for google");
        GoogleLoginOptions? google = EnvironmentSettings.GetGoogleLoginOptions(builder.Configuration);
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
        return builder;
    }
    
    public static WebApplication AddExceptionMiddleware(this WebApplication app, ILogger log)
    {
        if (app.Environment.IsDevelopment())
        {
            log.Information("Configure the HTTP request pipeline for DEVELOPMENT");
            app.UseMigrationsEndPoint();
            app.UseDeveloperExceptionPage();
        }
        else if (app.Environment.IsTest())
        {
            log.Information("Configure the HTTP request pipeline for DEVELOPMENT");
            app.UseMigrationsEndPoint();
            app.UseDeveloperExceptionPage();
        }
        else if (app.Environment.IsStaging())
        {
            log.Information("Configure the HTTP request pipeline for staging");
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }
        else if (app.Environment.IsDevelopment())
        {
            log.Information("Configure the HTTP request pipeline for staging");
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }
        else
            throw new Exception("Invalid application environment");
        return app;
    }

    public static WebApplication AddTransportSecurityMiddleware(this WebApplication app, ILogger log)
    {
        if ((app.Environment.IsProduction() || app.Environment.IsStaging()) && !EnvironmentSettings.IsRunningInContainer())
        {
            log.Information("Configure the HTTP transport security");
            app.UseHsts();
        }
        return app;
    }

    public static WebApplication AddRedirectionMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure the response compression");
        app.UseResponseCompression();

        if (!EnvironmentSettings.IsRunningInContainer())
        {
            log.Information("Configure HTTP redirection");
            app.UseHttpsRedirection();
        }
        return app;
    }

    public static WebApplication AddStaticFilesMiddleware(this WebApplication app, ILogger log)
    {
        if (app.Environment.IsStaging() || app.Environment.IsProduction())
        {
            log.Information("Configure using static files with caching");
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=86400"; //+ (int)(60 * 60 * 24);
                }
            });
        }
        else
        {
            log.Information("Configure using static files");
            app.UseStaticFiles();
        }

        return app;
    }

    public static WebApplication AddRoutingMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure request localization amd cookies");
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            SupportedCultures = EnvironmentSettings.GetSupportedCultures(),
            SupportedUICultures = EnvironmentSettings.GetSupportedCultures(),
            DefaultRequestCulture = new RequestCulture(EnvironmentSettings.GetDefaultCulture())
        });
        app.UseCookiePolicy();

        if (EnvironmentSettings.IsRunningInContainer())
        {
            log.Information("Configure forward headers");
            app.UseForwardedHeaders();
        }

        app.UseSerilogRequestLogging();
        app.UseRouting();

        return app;
    }

    public static WebApplication AddCorsMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure CORS middleware");
        return app;
    }

    public static WebApplication AddSecurityMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure authentication and authorization");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.UseResponseCaching();

        return app;
    }

    public static WebApplication AddSessionMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure session middleware");
        return app;
    }

    public static WebApplication AddEndpointMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Mapping and routing razor components");
        app.MapRazorPages();
        app.MapControllers();
        return app;
    }
}
