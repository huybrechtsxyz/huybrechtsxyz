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

    public string ProductUrl { get; set; } = string.Empty;

    public string ProductSearch { get; set; } = string.Empty;

    public string UnitUrl { get; set; } = string.Empty;

    public string UnitSearch { get; set; } = string.Empty;

    public string RatesUrl { get; set; } = string.Empty;

    public string RatesSearch { get; set; } = string.Empty;
}