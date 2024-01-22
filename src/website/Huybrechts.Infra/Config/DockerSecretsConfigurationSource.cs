using Microsoft.Extensions.Configuration;

namespace Huybrechts.Infra.Config;

public class DockerSecretsConfigurationsSource : IConfigurationSource
{
    private readonly string _secretsDirectoryPath;
    private readonly string _colonPlaceholder;
    private readonly ICollection<string>? _allowedPrefixes;

    internal DockerSecretsConfigurationsSource(
        string secretsDirectoryPath,
        string colonPlaceholder,
        ICollection<string>? allowedPrefixes = null)
    {
        _secretsDirectoryPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath));
        _colonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder));
		_allowedPrefixes = allowedPrefixes;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
		return new DockerSecretsConfigurationProvider(
            _secretsDirectoryPath,
            _colonPlaceholder,
            _allowedPrefixes);
    }
}