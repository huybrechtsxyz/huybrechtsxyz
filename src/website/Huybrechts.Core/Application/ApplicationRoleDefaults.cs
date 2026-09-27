using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Application;

public enum ApplicationSystemRole
{
    [Display(Name = nameof(Administrator), Description = nameof(Administrator) + "_d", ResourceType = typeof(Localization))]
    Administrator,

    [Display(Name = nameof(User), Description = nameof(User) + "_d", ResourceType = typeof(Localization))]
    User
}

public enum ApplicationTenantRole
{
    [Display(Name = nameof(None), Description = nameof(None) + "_d", ResourceType = typeof(Localization))]
    None,

    [Display(Name = nameof(Owner), Description = nameof(Owner) + "_d", ResourceType = typeof(Localization))]
    Owner,

    [Display(Name = nameof(Manager), Description = nameof(Manager) + "_d", ResourceType = typeof(Localization))]
    Manager,

    [Display(Name = nameof(Contributor), Description = nameof(Contributor) + "_d", ResourceType = typeof(Localization))]
    Contributor,

    [Display(Name = nameof(Member), Description = nameof(Member) + "_d", ResourceType = typeof(Localization))]
    Member,

    [Display(Name = nameof(Guest), Description = nameof(Guest) + "_d", ResourceType = typeof(Localization))]
    Guest
}
