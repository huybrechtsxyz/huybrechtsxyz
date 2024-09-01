using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents information about different countries, including their codes, names, and associated details.
/// </summary>
/// <remarks>
/// This class provides essential information about each country, such as its ISO code, official language, currency, and more.
/// The ISOCode is always stored in uppercase to ensure consistency across various data operations.
/// </remarks>
[MultiTenant]
[Table("SetupCountry")]
[Index(nameof(Code), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(SearchIndex))]
[Comment("Represents information about different countries, including their codes, names, and associated details.")]
public record SetupCountry : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the identifier of the official language of the country.
    /// </summary>
    public Ulid? SetupLanguageId { get; set; }

    /// <summary>
    /// Navigation property for the official language of the country.
    /// </summary>
    public SetupLanguage? SetupLanguage { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the official currency of the country.
    /// </summary>
    public Ulid? SetupCurrencyId { get; set; }

    /// <summary>
    /// Navigation property for the official currency of the country.
    /// </summary>
    public SetupCurrency? SetupCurrency { get; set; }

    /// <summary>
    /// Gets or sets the ISO 3166-1 alpha-2 code of the country.
    /// </summary>
    /// <remarks>
    /// The code should be in uppercase (e.g., "US" for United States, "DE" for Germany).
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the English short name of the country.
    /// </summary>
    /// <remarks>
    /// This property provides the commonly used name of the country in English.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the translated name of the country in its native language or most widely accepted form.
    /// </summary>
    /// <remarks>
    /// This property is useful for displaying the country's name as commonly used in the native language or in local usage.
    /// </remarks>
    [MaxLength(128)]
    public string TranslatedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief description of the country.
    /// </summary>
    /// <remarks>
    /// This property provides additional context or information about the country, such as its geographical location, population, or other relevant data.
    /// </remarks>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
