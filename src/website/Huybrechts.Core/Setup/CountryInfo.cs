using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents information about different countries, including their codes, names, and associated details.
/// </summary>
/// <remarks>
/// This class provides essential information about each country, such as its ISO code, official language, currency, and more.
/// The ISOCode is always stored in uppercase to ensure consistency across various data operations.
/// </remarks>
public class CountryInfo
{
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
    public string ShortName { get; set; } = string.Empty;

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
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default currency code of the country (ISO 4217 currency code).
    /// </summary>
    /// <remarks>
    /// This property provides the default currency used in the country (e.g., "USD" for United States Dollar, "EUR" for Euro).
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default language code of the country (ISO 639-1 language code).
    /// </summary>
    /// <remarks>
    /// This property provides the default or primary language used in the country (e.g., "EN" for English, "FR" for French).
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string LanguageCode { get; set; } = string.Empty;
}
