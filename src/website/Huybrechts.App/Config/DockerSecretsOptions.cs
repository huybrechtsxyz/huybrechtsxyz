﻿namespace Huybrechts.App.Config;

public sealed class DockerSecretsOptions
{
    public string SecretsPath { get; set; } = string.Empty;

    public string ColonPlaceholder {  get; set; } = string.Empty;

    public ICollection<string>? AllowedPrefixes {  get; set; }
}
