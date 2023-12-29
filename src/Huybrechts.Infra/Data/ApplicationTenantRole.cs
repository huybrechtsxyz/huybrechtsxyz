using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Infra.Data;

[Table("IdentityTenantRole")]
[Index(nameof(Name), IsUnique = true)]
public record ApplicationTenantRole
{
    [Key]
    [StringLength(450)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string ApplicationTenantCode { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public string? Remark { get; set; }

    [Timestamp]
    public byte[]? ConcurrencyToken { get; set; }

    public ICollection<ApplicationTenantUserRole>? UserRoles { get; set; }

    public enum DefaultRole
    {
        None = 0,
        Guest = 1,
        Member = 2,
        Contributor = 4,
        Manager = 8,
        Administrator = 16,
        Owner = 128
    }

    public static List<ApplicationTenantRole> GetDefaultRoles(ApplicationTenant tenant)
    {
        List<ApplicationTenantRole> list = [];
        foreach (var value in Enum.GetValues(typeof(DefaultRole)).Cast<DefaultRole>())
        {
            var item = new ApplicationTenantRole()
            {
                ApplicationTenantCode = tenant.Code,
                Name = value.ToString()
            };
            list.Add(item);
        }
        return list;
    }
}

