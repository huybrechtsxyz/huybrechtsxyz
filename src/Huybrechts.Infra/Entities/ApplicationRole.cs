using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Huybrechts.Infra.Data;

namespace Huybrechts.Infra.Entities;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
    private const string Hashtag = "#";
    public static readonly string SystemAdministrator = "Administrator";
    public static readonly string SystemUser = "User";

	public static List<ApplicationRole> GetDefaultRoles(string tenant)
	{
		List<ApplicationRole> list = [];
		foreach (var value in Enum.GetValues(typeof(ApplicationRoleValues)).Cast<ApplicationRoleValues>())
		{
			var item = new ApplicationRole()
			{
				Name = ApplicationRole.GetRoleName(tenant, value.ToString()),
				TenantId = tenant,
				Label = value.ToString()
			};
			if (value != ApplicationRoleValues.None)
				list.Add(item);
		}
		return list;
	}

	public static string GetTenantId(string rolename)
	{
        if (rolename.StartsWith(Hashtag))
		    return string.Empty;
        return rolename[0..(rolename.IndexOf(Hashtag) - 1)];
    }

	public static string GetRoleLabel(string rolename)
	{
        return rolename.StartsWith(Hashtag) ? rolename : rolename[(rolename.IndexOf(Hashtag) + 1)..];
    }

    public static string GetRoleName(string tenant, string label)
    {
        return $"{tenant}{Hashtag}{label}";
    }

	public static string GetTenantRole(string tenant, string role)
	{
		if (role.Contains(Hashtag))
			return role;
		return GetRoleName(tenant, role);
	}

    public ApplicationRole() : base() { }

	public ApplicationRole(string rolename) : base(rolename) { }

	public ApplicationRole(string tenant, string rolename) : base(GetRoleName(tenant, rolename)) { }

	[RegularExpression("^[a-z0-9]+$")]
	[StringLength(24, MinimumLength = 2)]
	public string? TenantId { get; set; }

	[Required]
	public string Label { get; set; } = string.Empty;

	[StringLength(512)]
	public string? Description { get; set; }

	public ApplicationRoleValues GetRoleValue()
	{
        if (Enum.TryParse<ApplicationRoleValues>(Label, out ApplicationRoleValues value))
            return value;
		return ApplicationRoleValues.None;
    }
}

public enum ApplicationRoleValues
{
    None = 0,
    Owner = 1,
    Manager = 4,
    Contributer = 8,
    Member = 16,
    Guest = 64
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property<string>("Id").HasColumnType("nvarchar(450)");
        builder.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
        builder.Property<string>("Name").HasMaxLength(256).HasColumnType("nvarchar(256)");
		builder.Property<string>("TenantId").HasMaxLength(24).HasColumnType("nvarchar(24)");
		builder.Property<string>("Label").HasMaxLength(200).HasColumnType("nvarchar(200)");
		builder.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.HasKey("Id");
        builder.HasIndex("TenantId", "Label").IsUnique().HasFilter("[Label] IS NOT NULL");
        builder.HasOne<ApplicationTenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Cascade);
        builder.ToTable("IdentityRole");
    }
}
