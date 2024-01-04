namespace Huybrechts.Infra.Config;

public class ApplicationSettings
{
    public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    public static bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");
}
