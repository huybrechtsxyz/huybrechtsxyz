using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Shared.Web.Config;

public static class ApplicationSettings
{
    private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

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

    public static bool IsRunningInContainer()
    {
        string value = (Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) ?? string.Empty).Trim().ToLower();
        return value.Equals("true") || value.Equals("1");
    }
}
