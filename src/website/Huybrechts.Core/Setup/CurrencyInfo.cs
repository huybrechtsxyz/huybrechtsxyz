using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a currency entity with detailed information such as code, name, description, and associated country code.
/// This class is used to manage currency-related data within the application and ensure consistency with ISO standards.
/// </summary>
/// <remarks>
/// The <c>CurrencyInfo</c> class is designed to be loaded into memory from a JSON file on application startup, 
/// providing a quick reference for currency-related operations without the need for frequent database access.
/// </remarks>
public class CurrencyInfo
{
    /// <summary>
    /// The ISO 4217 code for the currency (e.g., "USD" for US Dollar, "EUR" for Euro).
    /// This field is required and serves as the primary key.
    /// </summary>
    [Key]
    [Required(ErrorMessage = "Currency code is required.")]
    [MaxLength(10, ErrorMessage = "Currency code must not exceed 10 characters.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The ISO 3166-1 alpha-2 country code associated with the currency (e.g., "US" for the United States).
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Country code is required.")]
    [MaxLength(10, ErrorMessage = "Country code must not exceed 10 characters.")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// The name of the currency (e.g., "United States Dollar").
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Currency name is required.")]
    [MaxLength(128, ErrorMessage = "Currency name must not exceed 128 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the currency, which may include details like historical background or its usage.
    /// This field is optional.
    /// </summary>
    [MaxLength(256, ErrorMessage = "Description must not exceed 256 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// A concatenated and normalized string of key fields used for searching (e.g., "usd~united states dollar~us").
    /// This is a derived field and is not stored in the database.
    /// </summary>
    public string SearchIndex => $"{Code}~{Name}~{CountryCode}".ToLowerInvariant();
}