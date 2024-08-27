using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents the language information for different languages that can be supported or utilized by the application.
/// </summary>
/// <remarks>
/// This class is used to define and manage the language settings, such as code, name, and description. 
/// The Code property should always be stored in uppercase to maintain consistency across various data operations.
/// </remarks>
public record SetupLanguage : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the code of the language.
    /// This is typically a two-letter ISO language code (e.g., "EN" for English, "ES" for Spanish).
    /// </summary>
    /// <remarks>
    /// The code should be in uppercase to ensure consistency across different parts of the application.
    /// </remarks>
    [Required]
    [MaxLength(10, ErrorMessage = "The language code must be 10 characters long.")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "The language code must consist of two uppercase letters.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the language.
    /// This is the full name of the language (e.g., "English", "Spanish").
    /// </summary>
    /// <remarks>
    /// The name represents the human-readable form of the language identifier.
    /// </remarks>
    [Required]
    [MaxLength(128, ErrorMessage = "The language name must not exceed 128 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the language.
    /// This is the full name of the language (e.g., "English", "Spanish").
    /// </summary>
    /// <remarks>
    /// The name represents the human-readable form of the language identifier.
    /// </remarks>
    [Required]
    [MaxLength(128, ErrorMessage = "The language name must not exceed 128 characters.")]
    public string TranslatedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the language.
    /// Provides additional information about the language, such as its origins or primary usage areas.
    /// </summary>
    /// <remarks>
    /// This field can be used to store contextual or historical information about the language.
    /// </remarks>
    [MaxLength(256, ErrorMessage = "The language description must not exceed 256 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
