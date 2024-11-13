namespace Huybrechts.Shared.App.Config;

public class ApplicationSettings
{
    private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    public static bool IsRunningInContainer()
    {
        string value = (Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) ?? string.Empty).Trim().ToLower();
        return value.Equals("true") || value.Equals("1");
    }
}
