using Huybrechts.Shared.App.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Huybrechts.Shared.Web;

public static class WebHostExtensions
{
    public static bool IsTest(this IWebHostEnvironment env) => env.EnvironmentName == "Test";

    public static ILogger AddBootstrapLogger()
    {
       return new LoggerConfiguration()
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .CreateBootstrapLogger();
    }

    public static WebApplicationBuilder AddLoggingServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext(),
            writeToProviders: true);
        return builder;
    }
    public static IConfigurationBuilder AddDockerSecrets(
        this IConfigurationBuilder builder,
        IConfiguration configuration,
        ILogger log)
    {
        if (!ApplicationSettings.IsRunningInContainer())
            return builder;
        log.Information("Running application in container");
        log.Information("Reading configuration for docker secrets");
        DockerSecretsOptions options = ApplicationSettings.GetDockerSecretsOptions(configuration);
        return builder.Add(new DockerSecretsConfigurationsSource(options, log));
    }

    public static WebApplicationBuilder AddWebconfigServices(this WebApplicationBuilder builder, ILogger logger)
    {
        logger.Information("Configuring webserver");
        builder.Services.Configure<KestrelServerOptions>(builder.Configuration.GetSection(nameof(KestrelServerOptions)));

        if (ApplicationSettings.IsRunningInContainer())
        {
            logger.Information("Configuring forwarding headers");
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            logger.Information("Adding data protection keys");
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/app/data"))
                .SetApplicationName(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!);
        }

        builder.Services.AddResponseCaching();

        return builder;
    }
    
    public static WebApplicationBuilder AddCookiePolicies(this WebApplicationBuilder builder, ILogger logger)
    {
        logger.Information("Configuring cookie policy");
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
        });
        return builder;
    }

    public static WebApplication UseExceptionMiddleware(this WebApplication app, bool useMigrations, ILogger log)
    {
        if (app.Environment.IsDevelopment())
        {
            Log.Information("Configure the HTTP request pipeline for DEVELOPMENT");
            app.UseDeveloperExceptionPage();
        }
        else if (app.Environment.IsTest())
        {
            Log.Information("Configure the HTTP request pipeline for TEST");
            app.UseDeveloperExceptionPage();
        }
        else if (app.Environment.IsStaging())
        {
            Log.Information("Configure the HTTP request pipeline for STAGING");
            app.UseExceptionHandler("/Error");
        }
        else if (app.Environment.IsProduction())
        {
            Log.Information("Configure the HTTP request pipeline for PRODUCTION");
            app.UseExceptionHandler("/Error");
        }
        else
            throw new Exception("Invalid application environment for " + app.Environment.EnvironmentName);

        app.UseStatusCodePagesWithRedirects("/Error/{0}");

        return app;
    }

    public static WebApplication UseHstsMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure the HTTP transport security");
        return app;
    }

    public static WebApplication UseRedirecionMiddleware(this WebApplication app, ILogger log)
    {
        if (!ApplicationSettings.IsRunningInContainer())
        {
            log.Information("Configure HTTP redirection");
            app.UseHttpsRedirection();
        }
        return app;
    }

    public static WebApplication UseStaticFilesMiddleware(this WebApplication app, ILogger log)
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

    public static WebApplication UseLocalizationMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure request localization amd cookies");
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            SupportedCultures = ApplicationSettings.GetSupportedCultures(),
            SupportedUICultures = ApplicationSettings.GetSupportedCultures(),
            DefaultRequestCulture = new RequestCulture(ApplicationSettings.GetDefaultCulture())
        });
        app.UseCookiePolicy();
        return app;
    }

    public static WebApplication UseRoutingMiddleware(this WebApplication app, ILogger log)
    {
        if (ApplicationSettings.IsRunningInContainer())
        {
            log.Information("Configure forward headers");
            app.UseForwardedHeaders();
        }

        app.UseSerilogRequestLogging();
        app.UseRouting();

        return app;
    }

    public static WebApplication UseCorsMiddleware(this WebApplication app, ILogger log)
    {
        return app;
    }

    public static WebApplication UseClientSecurityMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure authentication and authorization");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.UseResponseCaching();
        return app;
    }

    public static WebApplication UseServerSecurityMiddleware(this WebApplication app, ILogger log)
    {
        log.Information("Configure authentication and authorization");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.UseResponseCaching();
        return app;
    }

    public static WebApplication UseCustomMiddleware(this WebApplication app, ILogger log)
    {
        return app;
    }
}
