using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a specific design or solution proposal for a project.
/// </summary>
/// <remarks>
/// The <c>ProjectDesign</c> class includes details about a proposed solution for a project. Each design can have multiple components and is used to find the right solution for the project.
/// </remarks>
[MultiTenant]
[Table("ProjectDesign")]
[Comment("Represents a specific design or solution proposal for a project.")]
[Index(nameof(ProjectInfoId), nameof(Name), IsUnique = true)]
[Index(nameof(SearchIndex))]
public record ProjectDesign : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the project associated with this design.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectInfo</c> entity, indicating which project this design is part of.
    /// </remarks>
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project information.
    /// </summary>
    /// <remarks>
    /// This navigation property links to the <c>ProjectInfo</c> class, providing access to the project's details associated with this design.
    /// </remarks>
    public ProjectInfo ProjectInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the project design.
    /// </summary>
    /// <remarks>
    /// This field represents the name given to this specific design for identification and display purposes.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the project design.
    /// </summary>
    /// <remarks>
    /// A detailed explanation of what this design entails, including any specific approaches, technologies, or considerations involved.
    /// </remarks>
    public string? Description { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
