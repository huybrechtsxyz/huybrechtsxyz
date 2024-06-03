using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Huybrechts.Infra.Config;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
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
        if (builder.Environment.IsDevelopment())
            return builder;

        string baseUri = EnvironmentSettings.GetApplicationHostUrl(builder.Configuration);
        if (string.IsNullOrEmpty(baseUri))
            throw new ApplicationException("Invalid Application Host URL defined");

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

        if (EnvironmentSettings.IsRunningInContainer())
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

        return builder;
    }

    public static WebApplicationBuilder AddXyzIdentity(this WebApplicationBuilder builder, ILogger log)
    {
        builder.Services.TryAddScoped<ApplicationUserStore>();
        builder.Services.TryAddScoped<ApplicationUserManager>();
        builder.Services.TryAddScoped<ApplicationSignInManager>();
        builder.Services.TryAddScoped<ApplicationUserClaimsPrincipalFactory>();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>, AuthenticationSender>();
        builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthenticationMailer>();

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
        .AddRoleValidator<ApplicationRoleValidator>()
        .AddRoleManager<ApplicationRoleManager>()
        .AddUserStore<ApplicationUserStore>()
        .AddUserManager<ApplicationUserManager>()
        .AddSignInManager<ApplicationSignInManager>()
        .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();
        return builder;
    }
}
