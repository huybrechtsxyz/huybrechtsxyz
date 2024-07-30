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
    [Display(Name = "State.New", Description = "State.New", ResourceType = typeof(ApplicationLocalization))]
    New = 1,        // A new tenant is created

    [Display(Name = "State.Pending", Description = "State.Pending", ResourceType = typeof(ApplicationLocalization))]
    Pending = 2,    // In progress to deploy resources

    [Display(Name = "State.Active", Description = "State.Active", ResourceType = typeof(ApplicationLocalization))]
    Active = 3,     // Resources deployed tenant usable

    [Display(Name = "State.Disabling", Description = "State.Disabling", ResourceType = typeof(ApplicationLocalization))]
    Disabling = 4,   // Set inactive by user

    [Display(Name = "State.Disabled", Description = "State.Disabled", ResourceType = typeof(ApplicationLocalization))]
    Disabled = 5,   // Set inactive by user

    [Display(Name = "State.Removing", Description = "State.Removing", ResourceType = typeof(ApplicationLocalization))]
    Removing = 6,   // Set to delete by user

    [Display(Name = "State.Removed", Description = "State.Removed", ResourceType = typeof(ApplicationLocalization))]
    Removed = 7     // Deleted by the system
}
