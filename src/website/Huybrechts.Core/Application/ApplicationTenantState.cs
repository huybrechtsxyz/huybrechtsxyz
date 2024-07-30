using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Application;

/// <summary>
/// New -> Pending
/// New -> Removing
/// Pending -> Active
/// Active -> Disabling
/// Disabling -> Disabled
/// Disabled  -> Pending
/// Disabled  -> Removing
/// Removing  -> Removed
/// </summary>
public enum ApplicationTenantState
{
    [Display(Name = "State_New", Description = "State_New", ResourceType = typeof(ApplicationLocalization))]
    New = 1,        // A new tenant is created

    [Display(Name = "State_Pending", Description = "State_Pending", ResourceType = typeof(ApplicationLocalization))]
    Pending = 2,    // In progress to deploy resources

    [Display(Name = "State_Active", Description = "State_Active", ResourceType = typeof(ApplicationLocalization))]
    Active = 3,     // Resources deployed tenant usable

    [Display(Name = "State_Disabling", Description = "State_Disabling", ResourceType = typeof(ApplicationLocalization))]
    Disabling = 4,   // Set inactive by user

    [Display(Name = "State_Disabled", Description = "State_Disabled", ResourceType = typeof(ApplicationLocalization))]
    Disabled = 5,   // Set inactive by user

    [Display(Name = "State_Removing", Description = "State_Removing", ResourceType = typeof(ApplicationLocalization))]
    Removing = 6,   // Set to delete by user

    [Display(Name = "State_Removed", Description = "State_Removed", ResourceType = typeof(ApplicationLocalization))]
    Removed = 7     // Deleted by the system
}
