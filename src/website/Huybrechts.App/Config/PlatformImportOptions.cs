namespace Huybrechts.App.Config;

public class PlatformImportOptions
{
    public Dictionary<string, PlatformImportOptionSettings> Platforms { get; set; } = [];
}

public class PlatformImportOptionSettings
{
    public string RegionUrl { get; set; } = string.Empty;

    public string RegionSearch { get; set; } = string.Empty;

    public string ServiceUrl { get; set; } = string.Empty;

    public string ServiceSearch { get; set; } = string.Empty;
}