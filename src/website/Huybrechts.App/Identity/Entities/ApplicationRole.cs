using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationRole")]
public sealed class ApplicationRole : IdentityRole
{
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string? TenantId { get; set; }

    [Required]
	[StringLength(256)]
	public string Label { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    //
    // EXTENDED
    //

    private const string Hashtag = "#";
    public static readonly string SystemAdministrator = "Administrator";
    public static readonly string SystemUser = "User";

    public static List<ApplicationRole> GetDefaultTenantRoles(string tenant)
    {
        List<ApplicationRole> list = [];
        foreach (var value in Enum.GetValues(typeof(ApplicationRoleValues)).Cast<ApplicationRoleValues>())
        {
            var item = new ApplicationRole()
            {
                Name = GetTenantRole(tenant, value),
                TenantId = tenant,
                Label = value.ToString()
            };
            if (value != ApplicationRoleValues.None)
                list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Returns the label of the role (without tenantid if applicable)
    /// </summary>
    /// <param name="roleid"></param>
    /// <returns>
	/// Application role? Complete rolename is returned.
	/// Tenant role? Label is returned without tenantid.
    /// </returns>
    public static string GetRoleLabel(string roleid)
    {
        return roleid.StartsWith(Hashtag) ? roleid : roleid[(roleid.IndexOf(Hashtag) + 1)..];
    }

    /// <summary>
    /// Returns the enumerated role to a string
    /// </summary>
    /// <param name="value"></param>
    /// <returns>
    /// The role enumeration as string
    /// </returns>
    public static string GetRoleName(ApplicationRoleValues value)
    {
        return value.ToString();
    }

    /// <summary>
    /// Returns the id of the tenant if available
    /// </summary>
    /// <param name="rolename"></param>
    /// <returns>
	/// Application role? Tenantid is Empty.
	/// Tenant role? Tenantid is returned.
	/// </returns>
    public static string GetTenantId(string roleid)
    {
        if (roleid.StartsWith(Hashtag))
            return string.Empty;
        return roleid[0..(roleid.IndexOf(Hashtag) - 1)];
    }

    /// <summary>
    /// Returns the name of the tenantrole based on tenant and role label
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="label"></param>
    /// <returns>
    /// Returns the created tenant-role
    /// </returns>
    public static string GetTenantRole(string tenant, string label)
    {
        return $"{tenant}{Hashtag}{label}";
    }

    /// <summary>
    /// Returns the name of the tenantrole based on tenant and role label
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="value"></param>
    /// <returns>
    /// Returns the created tenant-role
    /// </returns>
    public static string GetTenantRole(string tenant, ApplicationRoleValues value)
    {
        return $"{tenant}{Hashtag}{GetRoleName(value)}";
    }

    public ApplicationRole() : base() { }

    public ApplicationRole(string rolename) : base(rolename) { }

    public ApplicationRole(string tenant, string rolename) : base(GetTenantRole(tenant, rolename)) { }
}