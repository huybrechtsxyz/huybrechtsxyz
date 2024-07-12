using System.ComponentModel;

namespace Huybrechts.Core.Application;

public enum ApplicationTenantState
{
    [Description("New")]
    New = 1,        // A new tenant is created

    [Description("Pending")]
    Pending = 2,    // In progress to deploy resources

    [Description("Active")]
    Active = 3,     // Resources deployed tenant usable

    [Description("Disabling")]
    Disabling = 4,   // Set inactive by user

    [Description("Disabled")]
    Disabled = 5,   // Set inactive by user

    [Description("Removing")]
    Removing = 6,   // Set to delete by user

    [Description("Removed")]
    Removed = 7     // Deleted by the system
}
