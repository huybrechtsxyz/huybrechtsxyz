using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Application;

public record ApplicationTenantInfo
{
    [Key]
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string Id { get; set; } = string.Empty;

    public ApplicationTenantState State { get; set; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public string? Remark { get; set; }

    public byte[]? Picture { get; set; }

    [StringLength(32)]
    public string? DatabaseProvider { get; set; } = string.Empty;

    [StringLength(512)]
    public string? ConnectionString { get; set; }

    [Timestamp]
    public byte[]? ConcurrencyStamp { get; set; }

    public void UpdateFrom(ApplicationTenantInfo entity)
    {
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Remark = entity.Remark;
    }
}
