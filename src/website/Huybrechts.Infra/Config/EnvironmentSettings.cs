using Huybrechts.Infra.Data;
using Microsoft.Extensions.Configuration;

namespace Huybrechts.Infra.Config;

public static class EnvironmentSettings
{
    private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    private const string ENV_APP_HOST_URL = "APP_HOST_URL";
    private const string ENV_APP_HOST_EMAIL = "APP_HOST_EMAIL"; 
    private const string ENV_APP_HOST_USERNAME = "APP_HOST_USERNAME";
    private const string ENV_APP_HOST_PASSWORD = "APP_HOST_PASSWORD";

    private const string ENV_APP_DATA_URL = "APP_DATA_URL";
    private const string ENV_APP_DATA_NAME = "APP_DATA_NAME";
    private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
    private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";
    private const string ENV_APP_DATA_CONTEXT = "APP_DATA_CONTEXT";

    public static string GetApplicationHostUrl(IConfiguration configuration) => configuration.GetValue<string>(ENV_APP_HOST_URL) ?? string.Empty;

    public static string GetApplicationHostEmail(IConfiguration configuration) => configuration.GetValue<string>(ENV_APP_HOST_EMAIL) ?? string.Empty;

    public static string GetApplicationHostUsername(IConfiguration configuration) => configuration.GetValue<string>(ENV_APP_HOST_USERNAME) ?? string.Empty;

    public static string GetApplicationHostPassword(IConfiguration configuration) => configuration.GetValue<string>(ENV_APP_HOST_PASSWORD) ?? string.Empty;

    public static string GetRunningInContainer() => (Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) ?? string.Empty).Trim().ToLower();

    public static bool IsRunningInContainer() => GetRunningInContainer().Equals("true") || GetRunningInContainer().Equals("1");

    public static string GetContextConnectionString(IConfiguration configuration)
    {
        var dataUrl = configuration.GetValue<string>(ENV_APP_DATA_URL) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(dataUrl) || 1 == dataUrl.Length)
        {
            var dataContext = configuration.GetValue<string>(ENV_APP_DATA_CONTEXT) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(dataContext) && 1 < dataContext.Length)
            {
                dataUrl = configuration.GetConnectionString(dataContext);
            }
            else
                dataUrl = configuration.GetConnectionString(nameof(ApplicationContext));
        }

        if (string.IsNullOrEmpty(dataUrl))
            throw new ApplicationException($"Connection string is not found with APP_DATA_URL='{ENV_APP_DATA_URL}' and APPLICATIONCONTEXT");

        if (dataUrl.Contains("{database}", StringComparison.InvariantCultureIgnoreCase))
        {
            var database = configuration.GetValue<string>(ENV_APP_DATA_NAME) ?? string.Empty;
            if (!string.IsNullOrEmpty(database) && database.Length > 1)
                dataUrl = dataUrl.Replace("{database}", database, StringComparison.InvariantCultureIgnoreCase);
        }

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

    public static ContextProviderType GetContextProvider(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return ContextProviderType.None;

        // SQLite connection string pattern
        if (connectionString.Contains("Data Source=", StringComparison.InvariantCultureIgnoreCase))
            return ContextProviderType.Sqlite;

        // SQL Server connection string pattern
        else if (connectionString.Contains("Server=", StringComparison.InvariantCultureIgnoreCase))
            return ContextProviderType.SqlServer;

        // PostgreSQL connection string pattern
        //else if (connectionString.Contains("Host=") && connectionString.Contains("Port="))
        //	return ContextProviderType.Postgres;

        // Unable to determine database provider
        else
            return ContextProviderType.None;
    }
}
