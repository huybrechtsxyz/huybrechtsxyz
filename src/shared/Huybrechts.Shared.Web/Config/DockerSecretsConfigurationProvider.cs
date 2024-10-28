using Microsoft.Extensions.Configuration;
using Serilog;

namespace Huybrechts.Shared.Web.Config;

public sealed class DockerSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _secretsDirectoryPath;
    private readonly string _colonPlaceholder;
    private readonly ICollection<string>? _allowedPrefixes;
    private readonly ILogger _logger;

    public DockerSecretsConfigurationProvider(
        string secretsDirectoryPath,
        string colonPlaceholder,
        ICollection<string>? allowedPrefixes,
        ILogger logger)
    {
        _secretsDirectoryPath = secretsDirectoryPath ?? throw new ArgumentNullException(nameof(secretsDirectoryPath));
        _colonPlaceholder = colonPlaceholder ?? throw new ArgumentNullException(nameof(colonPlaceholder));
        _allowedPrefixes = allowedPrefixes;
        _logger = logger;
    }

    public override void Load()
    {
        if (!Directory.Exists(_secretsDirectoryPath))
        {
            _logger.Warning($"Unable to read docker secret path {_secretsDirectoryPath}");
            return;
        }

        foreach (string secretFilePath in Directory.EnumerateFiles(_secretsDirectoryPath))
        {
            _logger.Debug($"Reading configuration for docker secrets from {secretFilePath}");
            ProcessFile(secretFilePath);
        }
    }

    private void ProcessFile(string secretFilePath)
    {
        if (string.IsNullOrWhiteSpace(secretFilePath) || !File.Exists(secretFilePath))
        {
            _logger.Warning($"Unable to readi configuration for docker secrets from {secretFilePath}");
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
        _logger.Debug($"Addding configuration {secretKey} with {secretValue}");
    }
}