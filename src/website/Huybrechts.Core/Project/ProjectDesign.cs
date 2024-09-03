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
    /// Additional notes or remarks related to the project design.
    /// </summary>
    [Comment("Additional remarks or comments about the project design.")]
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the design.
    /// </summary>
    /// <remarks>
    /// Tags help categorize the design and improve searchability and filtering based on keywords.
    /// </remarks>
    [Comment("Keywords or categories for the design")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the current state of the project design.
    /// </summary>
    /// <remarks>
    /// Similar to the state field in Azure DevOps, this represents the current status of the project (e.g., New, Active, Resolved, Closed).
    /// </remarks>
    [Required]
    [MaxLength(32)]
    [Comment("Gets or sets the current state of the project design.")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for the current state of the design.
    /// </summary>
    /// <remarks>
    /// Provides context for the current state of the design.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the reason for the current state of the design.")]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the environment in which the project design is implemented.
    /// </summary>
    /// <remarks>
    /// This property specifies the environment (such as Development, Testing, Production) for which this project design is applicable.
    /// </remarks>
    [MaxLength(128)]
    [Comment("The environment in which the project design is implemented.")]
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the version of the design.
    /// </summary>
    /// <remarks>
    /// The version helps in tracking different iterations or updates of the design (e.g., "v1.0", "v2.1").
    /// </remarks>
    [MaxLength(32)]
    [Comment("Design version")]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the list of dependencies required for the design.
    /// </summary>
    /// <remarks>
    /// Lists other projects, systems, or resources that the design relies on, which is important for planning and risk assessment.
    /// </remarks>
    [Comment("List of dependencies for the design")]
    public string? Dependencies { get; set; }

    /// <summary>
    /// Gets or sets the priority of the project.
    /// </summary>
    /// <remarks>
    /// Represents the project's priority level (e.g., High, Medium, Low).
    /// </remarks>
    [MaxLength(32)]
    [Comment("Gets or sets the priority of the project.")]
    public string? Priority { get; set; }

    /// <summary>
    /// Gets or sets the risk of the project.
    /// </summary>
    /// <remarks>
    /// Represents the project's risk level (e.g., High, Medium, Low).
    /// </remarks>
    [MaxLength(32)]
    [Comment("Gets or sets the risk of the project.")]
    public string? Risk { get; set; }

    /// <summary>
    /// Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval.
    /// </summary>
    /// <remarks>
    /// This field is used to assign a rating to the project, which could be based on various factors such as priority, quality, or feedback from stakeholders. 
    /// The rating value is typically represented on a scale (e.g., 1 to 5) or a descriptive measure.
    /// </remarks>
    [Comment("Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval.")]
    public int? Rating { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}
