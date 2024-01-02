using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huybrechts.Infra.Data;

[Table("IdentityRole")]
public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string rolename) : base(rolename) { }

    public ApplicationRole(string tenant, string rolename) : base (rolename)
    {
        TenantId = tenant;
    }

    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string TenantId { get; set; } = string.Empty;

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
        builder.Property<string>("TenantId").HasMaxLength(24).HasColumnType("nvarchar(24)");
        builder.Property<string>("Name").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("nvarchar(256)");
        builder.HasKey("Id");
        builder.HasIndex("TenantId", "NormalizedName")
            .IsUnique()
            .HasDatabaseName("RoleNameIndex")
            .HasFilter("[TenantId] IS NOT NULL")
            .HasFilter("[NormalizedName] IS NOT NULL");
        builder.HasOne<ApplicationTenant>().WithMany().HasForeignKey("TenantId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityRole");
    }
}

public class ApplicationRoleValidator : IRoleValidator<ApplicationRole>
{
    public async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(role.Name))
        {
            var existingRole = await manager.Roles.FirstOrDefaultAsync(x => x.TenantId == role.TenantId && x.Name == role.Name);
            if (existingRole is null)
                return IdentityResult.Success;

            errors.Add(string.Format("{0} is already taken.", role.Name));
        }
        else
        {
            errors.Add("Name cannot be null or empty.");
        }

        if (errors.Count == 0)
            return IdentityResult.Success;

        IdentityError[] identityErrors = new IdentityError[errors.Count];
        errors.ToArray().CopyTo(identityErrors, 0);
        return IdentityResult.Failed(identityErrors);
    }
}

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    public ApplicationRoleManager(
        IRoleStore<ApplicationRole> store, 
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
        ILogger<RoleManager<ApplicationRole>> logger) 
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {

    }
}
