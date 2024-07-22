using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The resources of the platform
/// Azure -> APIm - Standard
/// One Platform has multiple Locations
/// </summary>
[MultiTenant]
[Table("PlatformResource")]
[DisplayName("Resource")]
public record PlatformResource
{
    [Key]
    [Required]
    [DisplayName("ID")]
    [Comment("PlatformResource PK")]
    public int Id { get; set;} = 0;

    [Required]
    [DisplayName("Provider ID")]
    [Comment("PlatformProvider FK")]
    public int PlatformProviderId { get; set;} = 0;

    [Required]
    [DisplayName("Service ID")]
    [Comment("PlatformService FK")]
    public int PlatformServiceId { get; set;} = 0;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    [DisplayName("Description")]
    [Comment("Description")]
    public string? Description { get; set; }

    [MaxLength(256)]
    [DisplayName("Cost Driver")]
    [Comment("Cost driver")]
    public string? CostDriver { get; set; }

    [MaxLength(128)]
    [DisplayName("Cost Based On")]
    [Comment("Cost based on")]
    public string? CostBasedOn { get; set; }

    [MaxLength(512)]
    [DisplayName("Limitations")]
    [Comment("Resource limitations")]
    public string? Limitations { get; set; }

    [MaxLength(512)]
    [DisplayName("About Link")]
    [Comment("Resource url")]
    public string? AboutURL { get; set; }

    [MaxLength(512)]
    [DisplayName("Pricing Link")]
    [Comment("Pricing url")]
    public string? PricingURL { get; set; }

    [MaxLength(64)]
    [DisplayName("Product ID")]
    [Comment("Product id")]
    public string? ProductId { get; set; }

    [MaxLength(64)]
    [DisplayName("Product")]
    [Comment("Product")]
    public string? Product { get; set; }

    [MaxLength(128)]
    [DisplayName("Size")]
    [Comment("Resource size")]
    public string? Size { get; set; }

    [DisplayName("Remarks")]
    [Comment("Remarks")]
    public string? Remarks { get; set; }

    /// <summary>
    /// Navigate to Rates
    /// </summary>
    public ICollection<PlatformRate> Rates { get; set; } = [];
}