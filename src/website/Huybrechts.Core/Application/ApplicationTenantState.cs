﻿namespace Huybrechts.Core.Application;

public enum ApplicationTenantState
{
    New = 1,        // A new tenantis created
    Pending = 2,    // In progress to deploy resources
    Active = 3,     // Resources deployed tenant usable
    Inactive = 4,   // Set inactive by user
    Removing = 5,   // Set to delete by user
    Removed = 6     // Deleted by the system
}
