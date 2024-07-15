using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The search parameters for looking up data in the platform
/// </summary>
[NotMapped]
public class SearchParameters
{
    [Required]
    public string? Service { get; set; }

    [Required]
    public string? Location { get; set; }

    [Required]
    public string? CurrencyCode { get; set; }
}