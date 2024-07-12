using Microsoft.Extensions.Configuration;

namespace Huybrechts.App.Config;

internal sealed class DockerSecretsConfigurationsSource : IConfigurationSource
{
    private readonly DockerSecretsOptions _options;

	internal DockerSecretsConfigurationsSource(
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
    }

    internal DockerSecretsConfigurationsSource(DockerSecretsOptions options)
    {
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DockerSecretsConfigurationProvider(
           _options.SecretsPath,
           _options.ColonPlaceholder,
           _options.AllowedPrefixes);
    }
}