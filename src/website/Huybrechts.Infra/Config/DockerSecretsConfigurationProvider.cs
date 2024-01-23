using Microsoft.Extensions.Configuration;

namespace Huybrechts.Infra.Config;

public class DockerSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _secretsDirectoryPath;
    private readonly string _colonPlaceholder;
    private readonly ICollection<string>? _allowedPrefixes;

    internal DockerSecretsConfigurationProvider(
        string secretsDirectoryPath,
        string colonPlaceholder,
        ICollection<string>? allowedPrefixes)
    {
        _secretsDirectoryPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath));
        _colonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder));
        _allowedPrefixes = allowedPrefixes;
    }

    public override void Load()
    {
        if (!Directory.Exists(_secretsDirectoryPath))
            return;

        foreach (string secretFilePath in Directory.EnumerateFiles(_secretsDirectoryPath))
        {
            ProcessFile(secretFilePath);
        }
    }

    private void ProcessFile(string secretFilePath)
    {
        if (string.IsNullOrWhiteSpace(secretFilePath) || !File.Exists(secretFilePath))
        {
            return;
        }

        string secretFileName = Path.GetFileName(secretFilePath);
        if (string.IsNullOrWhiteSpace(secretFileName))
        {
            return;
        }

        if (_allowedPrefixes is not null && _allowedPrefixes.Count > 0)
        {
            if (!_allowedPrefixes.Any(
                prefix => secretFileName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)
            ))
            {
                return;
            }
        }

		using var reader = new StreamReader(File.OpenRead(secretFilePath));
		string secretValue = reader.ReadToEnd();
		if (secretValue.EndsWith(Environment.NewLine))
		{
			secretValue = secretValue[..^1];
		}

		string secretKey = secretFileName.Replace(_colonPlaceholder, ":");
		Data.Add(secretKey, secretValue);
	}
}