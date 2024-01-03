using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huybrechts.Infra.Entities;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
    public static readonly string SystemAdministrator = "sysadmin";
    public static readonly string SystemUser = "user";

    public static string GetTenantId(string rolename)
	{
		if (rolename.IndexOf("#") == 0)
		    return string.Empty;
	    return rolename[0..(rolename.IndexOf("#") - 1)];
    }

	public static string GetRoleLabel(string rolename)
	{
		if (rolename.IndexOf("#") == 0)
			return rolename;
		return rolename[(rolename.IndexOf("#") + 1)..];
	}

    public static string GetRoleName(string tenant, string label)
    {
        return $"{tenant}#{label}";
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

	public static List<ApplicationRole> GetDefaultRoles()
	{
		List<ApplicationRole> list = [];
		foreach (var value in Enum.GetValues(typeof(ApplicationRoleValues)).Cast<ApplicationRoleValues>())
		{
			var item = new ApplicationRole()
			{
				Name = value.ToString()
			};
			if (value != ApplicationRoleValues.None)
				list.Add(item);
		}
		return list;
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
