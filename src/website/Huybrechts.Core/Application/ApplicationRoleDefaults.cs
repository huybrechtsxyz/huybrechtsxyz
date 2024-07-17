using System.ComponentModel;

namespace Huybrechts.Core.Application;

public enum ApplicationSystemRole
{
    [Description("The system administrator role")]
    Administrator,

    [Description("The standard user role")]
    User
}

public enum ApplicationTenantRole
{
    [Description("A role that does not provide any access")]
    None,

    [Description("The owner has administrator access to the tenant")]
    Owner,

    [Description("The manager can maintain privileged of the tenant")]
    Manager,

    [Description("The contributer has read/write access to the data of the tenant")]
    Contributer,

    [Description("The contributer can comment on the data of the tenant")]
    Member,

    [Description("The guest can read the data of the tenant")]
    Guest
}
