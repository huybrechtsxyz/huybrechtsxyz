using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The rate of a resource on a platform
/// </summary>
[Table("PlatformRate")]
[DisplayName("Rate")]
public record PlatformRate
{
    [Key]
    [Required]
    [DisplayName("Rate ID")]
    [Comment("Primary Key")]
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
    [DisplayName("Resource ID")]
    [Comment("PlatformResource FK")]
    public int PlatformResourceId { get; set;} = 0;

    [Required]
    [DisplayName("Location ID")]
    [Comment("PlatformLocation FK")]
    public int PlatformLocationId { get; set;} = 0;

    [Required]
    [MaxLength(10)]
    [DisplayName("Currency Code")]
    [Comment("Currency code")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [DisplayName("Name")]
    [Comment("Name")]
    public string Name { get; set;} = string.Empty;

    [DisplayName("Valid From")]
    [Comment("Rate is valid from")]
    public DateTime ValidFrom { get; set; }

    [Precision(12, 4)]
    [DisplayName("Retail Price")]
    [Comment("Retail price")]
    public decimal RetailPrice { get; set; }

    [Precision(12, 4)]
    [DisplayName("Unit Price")]
    [Comment("Unit price")]
    public decimal UnitPrice { get; set; }

    [Precision(12, 4)]
    [DisplayName("Mininum Units")]
    [Comment("Tier mininum units")]
    public decimal MininumUnits { get; set; } = 0;

    [MaxLength(32)]
    [DisplayName("Unit of Measure")]
    [Comment("Azure rate unit of measure")]
    public string? UnitOfMeasure { get; set; }

    [MaxLength(128)]
    [DisplayName("Product")]
    [Comment("Product")]
    public string? Product { get; set; }

    [MaxLength(64)]
    [DisplayName("Meter ID")]
    [Comment("Meter id")]
    public string? MeterId { get; set; }

    [MaxLength(128)]
    [DisplayName("Meter Name")]
    [Comment("Meter name")]
    public string? MeterName { get; set; }

    [MaxLength(64)]
    [DisplayName("Sku ID")]
    [Comment("Sku id")]
    public string? SkuId { get; set; }

    [MaxLength(128)]
    [DisplayName("Sku")]
    [Comment("Sku name")]
    public string? Sku { get; set; }

    [MaxLength(128)]
    [DisplayName("Rate Type")]
    [Comment("Rate type")]
    public string? Type { get; set; }

    [DisplayName("Is Primary")]
    [Comment("Is primary meter region")]
    public bool? IsPrimaryRegion { get; set; }

    [DisplayName("Remark")]
    [Comment("Remark")]
    public string? Remark { get; set; }

    /// <summary>
    /// Navigate to Rate Units
    /// </summary>
    public ICollection<PlatformRateUnit> RateUnits { get; set; } = [];
}