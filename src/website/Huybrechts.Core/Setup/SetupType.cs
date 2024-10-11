using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a setup entity that defines a specific type or classification for an object.
/// This class is used to categorize various objects within the system based on their types and associated codes.
/// </summary>
/// <remarks>
/// The <c>SetupTypeOf</c> class provides a way to manage and configure different object types and their corresponding values.
/// It includes fields for defining the object source, type, value, and additional descriptive information.
/// </remarks>
[MultiTenant]
[Table("SetupType")]
[Index(nameof(TenantId), nameof(TypeOf), nameof(Name))]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Defines various object types and codes within the setup configuration.")]
public record SetupType : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of field (e.g., ProjectType, ProjectKind).
    /// </summary>
    /// <remarks>
    /// This field defines the category or classification type for the object.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The classification or type for the object (e.g., ProjectType, ProjectKind).")]
    public string TypeOf { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value that will be generated for the type.
    /// </summary>
    /// <remarks>
    /// This field stores the generated code or value that uniquely identifies the type.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The code or value representing this type (e.g., X00 - X01).")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the type.
    /// </summary>
    /// <remarks>
    /// This field provides additional information or context for the type, such as its purpose or background.
    /// </remarks>
    [MaxLength(256)]
    [Comment("A detailed description providing additional context or information about the type.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the search index, which stores the normalized concatenated values for efficient searching.
    /// </summary>
    /// <remarks>
    /// This field is used to optimize search operations by storing a normalized version of key fields.
    /// </remarks>
    [Comment("Stores normalized, concatenated values for efficient searching.")]
    public string? SearchIndex { get; set; }
}
