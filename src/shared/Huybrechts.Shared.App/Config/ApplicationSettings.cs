using Huybrechts.Shared.App.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Shared.App.Config;

public static class ApplicationSettings
{
    private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
    private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";
    private const string ENV_APP_DATA_CONTEXT = "APP_DATA_CONTEXT";

    public static string GetContextConnectionString(IConfiguration configuration)
    {
        var dataUrl = string.Empty;

        var dataContext = configuration.GetValue<string>(ENV_APP_DATA_CONTEXT) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(dataContext) && 1 < dataContext.Length)
        {
            dataUrl = configuration.GetConnectionString(dataContext);
        }
        //else
        //    dataUrl = configuration.GetConnectionString(nameof(ApplicationContext));

        if (string.IsNullOrEmpty(dataUrl))
            throw new ApplicationException($"Connection string is not found with for 'ApplicationContext'");

        if (dataUrl.Contains("{username}", StringComparison.InvariantCultureIgnoreCase))
        {
            var username = configuration.GetValue<string>(ENV_APP_DATA_USERNAME) ?? string.Empty;
            if (!string.IsNullOrEmpty(username) && username.Length > 1)
                dataUrl = dataUrl.Replace("{username}", username, StringComparison.InvariantCultureIgnoreCase);
        }

        if (dataUrl.Contains("{password}", StringComparison.InvariantCultureIgnoreCase))
        {
            var password = configuration.GetValue<string>(ENV_APP_DATA_PASSWORD) ?? string.Empty;
            if (!string.IsNullOrEmpty(password) && password.Length > 1)
                dataUrl = dataUrl.Replace("{password}", password, StringComparison.InvariantCultureIgnoreCase);
        }

        return dataUrl;
    }

    public static DockerSecretsOptions GetDockerSecretsOptions(IConfiguration configuration)
    {
        DockerSecretsOptions? options = new();
        configuration.GetSection(nameof(DockerSecretsOptions)).Bind(options);
        return options ?? new()
        {
            SecretsPath = "/run/secrets",
            ColonPlaceholder = "__",
            AllowedPrefixes = []
        };
    }

    public static CultureInfo GetDefaultCulture() => new("EN");

    public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

    public static SupportedContextProvider GetSupportedContextProvider(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return SupportedContextProvider.None;

        // SQL Server connection string pattern
        else if (connectionString.Contains("Server=", StringComparison.InvariantCultureIgnoreCase))
        {
            if (connectionString.Contains("mssqllocaldb;", StringComparison.InvariantCultureIgnoreCase)
                && connectionString.Contains("(localdb)", StringComparison.InvariantCultureIgnoreCase))
                return SupportedContextProvider.SqlLite;
            else
                return SupportedContextProvider.SqlServer;
        }

        // PostgreSQL connection string pattern
        else if (connectionString.Contains("Host=") && connectionString.Contains("Database="))
            return SupportedContextProvider.Postgres;

        // Unable to determine database provider
        else
            return SupportedContextProvider.None;
    }

    public static bool IsRunningInContainer()
    {
        string value = (Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) ?? string.Empty).Trim().ToLower();
        return value.Equals("true") || value.Equals("1");
    }
}
