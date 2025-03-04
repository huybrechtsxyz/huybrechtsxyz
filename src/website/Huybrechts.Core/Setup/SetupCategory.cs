using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a setup entity that allows the categorization and subcategorization of any object.
/// This class can be used as a lookup for selecting either a category or subcategory, depending on the selection of a category.
/// </summary>
/// <remarks>
/// The <c>SetupCategory</c> class enables the assignment of both category and subcategory fields to various objects within the system.
/// It is designed to provide hierarchical categorization where subcategories are dependent on the category selected.
/// </remarks>
[MultiTenant]
[Table("SetupCategory")]
[Index(nameof(TenantId), nameof(TypeOf), nameof(Category), nameof(Subcategory))]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Defines categories and subcategories for objects in the setup configuration.")]
public record SetupCategory : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of field (e.g., ProjectCategory).
    /// </summary>
    /// <remarks>
    /// This field defines the category or classification type for the object.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The classification or type for the object (e.g., ProjectCategory).")]
    public string TypeOf { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category value.
    /// </summary>
    /// <remarks>
    /// This field is used to assign a primary category to the object.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The primary category assigned to the object.")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subcategory value.
    /// </summary>
    /// <remarks>
    /// This field is used to assign a subcategory to the object, which is typically dependent on the selected category.
    /// </remarks>
    [MaxLength(64)]
    [Comment("The subcategory assigned to the object, dependent on the selected category.")]
    public string Subcategory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description providing additional details about the category or subcategory.
    /// </summary>
    /// <remarks>
    /// This field can store a detailed description or contextual information about the category and subcategory.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Additional details or context for the category or subcategory.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the search index, which stores the normalized concatenated values for efficient searching.
    /// </summary>
    /// <remarks>
    /// This field is used to optimize search operations by storing a normalized version of key fields for quick lookup.
    /// </remarks>
    [Comment("Stores normalized, concatenated values for efficient searching.")]
    public string? SearchIndex { get; set; }
}
