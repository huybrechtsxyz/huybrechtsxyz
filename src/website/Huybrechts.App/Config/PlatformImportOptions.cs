namespace Huybrechts.App.Config;

public class PlatformImportOptions
{
    public Dictionary<string, PlatformImportOptionSettings> Platforms { get; set; } = [];
}

public class PlatformImportOptionSettings
{
    public string Regions { get; set; } = string.Empty;

    public string Locations { get; set; } = string.Empty;
}