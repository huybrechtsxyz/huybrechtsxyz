using Microsoft.Extensions.Configuration;
using Serilog;

namespace Huybrechts.App.Config;

internal sealed class DockerSecretsConfigurationsSource : IConfigurationSource
{
    private readonly DockerSecretsOptions _options;
    private readonly ILogger _logger;

    internal DockerSecretsConfigurationsSource(
        ILogger logger,
        string secretsDirectoryPath,
        string colonPlaceholder,
        ICollection<string>? allowedPrefixes = null)
    {
        _options = new DockerSecretsOptions()
        {
            SecretsPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath)),
            ColonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder)),
            AllowedPrefixes = allowedPrefixes
        };
        _logger = logger;
    }

    internal DockerSecretsConfigurationsSource(DockerSecretsOptions options, ILogger logger)
    {
        _options = options;
        _logger = logger;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DockerSecretsConfigurationProvider(
           _options.SecretsPath,
           _options.ColonPlaceholder,
           _options.AllowedPrefixes,
           _logger);
    }
}