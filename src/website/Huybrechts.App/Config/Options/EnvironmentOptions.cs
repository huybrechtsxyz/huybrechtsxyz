using Huybrechts.App.Data;

namespace Huybrechts.App.Config.Options;

public sealed class EnvironmentOptions
{
	public DatabaseProviderType DatabaseProviderType { get; set; } = DatabaseProviderType.None;

	public EnvironmentInitialization EnvironmentInitialization { get; set; } = EnvironmentInitialization.None;

	public bool DoInitializeEnvironment() => EnvironmentInitialization == EnvironmentInitialization.Initialize;

	public bool DoResetEnvironment() => EnvironmentInitialization == EnvironmentInitialization.Reset;
}
