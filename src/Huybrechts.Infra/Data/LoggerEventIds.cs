using Microsoft.Extensions.Logging;

namespace Huybrechts.Shared.Logging;

public static class LoggerEventIds
{
	public static readonly EventId RoleValidationFailed = new EventId(0, "RoleValidationFailed");
}
