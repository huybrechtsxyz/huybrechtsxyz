
public sealed class ApplicationSettings2
{

    public static readonly string ENV_APP_DATA_TYPE = "APP_DATA_TYPE";
    public static readonly string ENV_APP_DATA_URL = "APP_DATA_URL";
    public static readonly string ENV_APP_DATA_NAME = "APP_DATA_NAME";

    public string GetApplicationConnectionString2()
    {
        string? dataUrl = Environment.GetEnvironmentVariable(ENV_APP_DATA_URL);
        if (string.IsNullOrEmpty(dataUrl))
        {
            if (_options is not null && !string.IsNullOrEmpty(_options.ConnectionString))
                dataUrl = _options.ConnectionString;
            else
                dataUrl = _configuration.GetConnectionString("DatabaseContext");
        }
        if (dataUrl is null)
            throw new InvalidOperationException("Connection string for DatabaseContext not found.");

        var dbname = Environment.GetEnvironmentVariable(ENV_APP_DATA_NAME);
        if (!string.IsNullOrEmpty(dbname) && dataUrl.Contains("{database}"))
        {
            if (dbname.ToUpper().EndsWith("_FILE") || dbname.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
                dbname = File.ReadAllText(dbname);
            dataUrl = dataUrl.Replace("{database}", dbname);
        }

        var user = Environment.GetEnvironmentVariable(_configuration["Environment:Username"] ?? string.Empty);
        if (!string.IsNullOrEmpty(user) && dataUrl.Contains("{username}"))
        {
            if (user.ToUpper().EndsWith("_FILE") || user.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
                user = File.ReadAllText(user);
            dataUrl = dataUrl.Replace("{username}", user);
        }

        var pass = Environment.GetEnvironmentVariable(_configuration["Environment:Password"] ?? string.Empty);
        if (!string.IsNullOrEmpty(pass) && dataUrl.Contains("{password}"))
        {
            if (pass.ToUpper().EndsWith("_FILE") || pass.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
                pass = File.ReadAllText(pass);
            dataUrl = dataUrl.Replace("{password}", pass);
        }

        return dataUrl;
    }

    public DatabaseProviderType GetApplicationConnectionType2()
    {
        string? provider = Environment.GetEnvironmentVariable(ENV_APP_DATA_TYPE);
        if (string.IsNullOrEmpty(provider))
        {
            if (_options is not null && _options.DatabaseProviderType != DatabaseProviderType.None)
                return _options.DatabaseProviderType;

            provider = _configuration["Environment:DatabaseType"];
            if (string.IsNullOrEmpty(provider))
                throw new InvalidOperationException("DatabaseProviderType not found.");
        }

        if (Enum.TryParse(provider, out DatabaseProviderType dbtype))
            return dbtype;

        throw new InvalidCastException($"Invalid DatabaseProviderType for {provider}");
    }



}
