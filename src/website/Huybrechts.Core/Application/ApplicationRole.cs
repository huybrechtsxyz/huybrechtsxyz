using Huybrechts.Core.Extensions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Huybrechts.Core.Application;

/// <summary>
/// Fields in IdentityRole that apply
/// public override string? Name { get; set; }
/// public virtual string? NormalizedName { get; set; }
/// 
/// Role name is determined by
/// System role: {system-role}
/// Tenant role: {tenant-id#tenant-role}
/// </summary>
[Table("ApplicationRole")]
public sealed class ApplicationRole : IdentityRole
{
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24)]
    public string? TenantId { get; set; }

    [Required]
    [StringLength(256)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string NormalizedLabel { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public bool IsTenantRole() => !string.IsNullOrEmpty(TenantId);

    public bool IsSystemRole() => string.IsNullOrEmpty(TenantId);

    public ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = [];
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public ApplicationRole() : base() { }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="value">The name of the role (without tenant)</param>
    public ApplicationRole(string value) : base(value)
    {
        Set(value);
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="tenant">Name of the tenant</param>
    /// <param name="value">Name of the role</param>
    public ApplicationRole(string tenant, string value)
    {
        Set(tenant, value);
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="tenant">Name of the tenant</param>
    /// <param name="value">The default system role</param>
    public ApplicationRole(ApplicationSystemRole value)
        : this(value.ToString())
    {
        Description = value.GetDescription();
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="tenant">Name of the tenant</param>
    /// <param name="value">The default tenant role</param>
    public ApplicationRole(string tenant, ApplicationTenantRole value)
        : this(tenant, value.ToString())
    {
        Description = value.GetDescription();
    }

    public void Set(string roleName)
    {
        Name = roleName.Trim();
        TenantId = GetTenant(roleName);
        Label = GetLabel(roleName);
        NormalizedLabel = Label.ToUpper();
    }

    public void Set(string tenant, string label)
    {
        Name = GetRoleName(tenant, label);
        TenantId = tenant.Trim().ToLower();
        Label = label.Trim();
        NormalizedLabel = Label.ToUpper();
    }

    //
    // ROLE / TENANT / LABEL MANAGEMENT
    //

    private const string Hashtag = "#";

    public static List<ApplicationRole> GetDefaultSystemRoles()
    {
        List<ApplicationRole> list = [];
        foreach (var value in Enum.GetValues(typeof(ApplicationSystemRole)).Cast<ApplicationSystemRole>())
        {
            var item = new ApplicationRole(value);
            list.Add(item);
        }
        return list;
    }

    public static List<ApplicationRole> GetDefaultTenantRoles(string tenant)
    {
        List<ApplicationRole> list = [];
        foreach (var value in Enum.GetValues(typeof(ApplicationTenantRole)).Cast<ApplicationTenantRole>())
        {
            var item = new ApplicationRole(tenant, value);
            if (value != ApplicationTenantRole.None)
                list.Add(item);
        }
        return list;
    }

    public static string GetRoleName(ApplicationSystemRole role) => role.ToString();

	public static string GetNormalizedRoleName(ApplicationSystemRole role) => role.ToString().Trim().ToUpper();

	public static string GetRoleName(string tenant, ApplicationTenantRole role) => GetRoleName(tenant, role.ToString());

    public static string GetRoleName(string tenant, string label) => $"{tenant.Trim().ToLower()}{Hashtag}{label.Trim()}";

    public static string GetLabel(string roleName) => roleName.StartsWith(Hashtag) ? roleName.Trim() : roleName[(roleName.IndexOf(Hashtag) + 1)..].Trim();

    public static string GetTenant(string roleName) => !roleName.Contains(Hashtag) ? string.Empty : roleName[0..(roleName.IndexOf(Hashtag))].Trim().ToLower();
}
