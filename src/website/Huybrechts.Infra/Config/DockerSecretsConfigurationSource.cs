using Microsoft.Extensions.Configuration;

namespace Huybrechts.Infra.Config;

internal sealed class DockerSecretsConfigurationsSource : IConfigurationSource
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

	IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder)
	{
		return new DockerSecretsConfigurationProvider(
			_secretsDirectoryPath,
			_colonPlaceholder,
			_allowedPrefixes);
	}
}