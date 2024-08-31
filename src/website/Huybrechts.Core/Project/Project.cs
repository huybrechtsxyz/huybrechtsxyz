using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a project that can be created and managed on the platform.
/// </summary>
/// <remarks>
/// This class is the core entity for managing projects, allowing architects to create and define the scope, design,
/// scenarios, and simulations necessary to estimate costs and resources for different platform solutions.
/// </remarks>
[MultiTenant]
[Table("Project")]
[Index(nameof(Code), IsUnique = true)]
[Index(nameof(SearchIndex))]
[Comment("Table storing information about Projects that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.")]
public record ProjectInfo : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the identifier of the parent project, if this project is a sub-project or related to another project.
    /// </summary>
    /// <remarks>
    /// This field allows the structuring of projects in a hierarchical manner, similar to how Epics can have parent-child relationships in Azure DevOps.
    /// The ParentId is an optional field and can be null if the project does not have a parent.
    /// </remarks>
    public Ulid? ParentId { get; set; }


    /// <summary>
    /// The unique code of the Project.
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Comment("Code of the Project.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Project (e.g., Azure, Google Cloud, On-Premise).
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the Project.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the Project, detailing its purpose or features.
    /// </summary>
    [MaxLength(256)]
    [Comment("Detailed description of the Project.")]
    public string? Description { get; set; }

    /// <summary>
    /// Additional notes or remarks related to the Project.
    /// </summary>
    [Comment("Additional remarks or comments about the Project.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Gets or sets the current state of the project.
    /// </summary>
    /// <remarks>
    /// Similar to the state field in Azure DevOps, this represents the current status of the project (e.g., New, Active, Resolved, Closed).
    /// </remarks>
    [Required]
    [MaxLength(32)]
    [Comment("Gets or sets the current state of the project.")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for the current state of the project.
    /// </summary>
    /// <remarks>
    /// Provides context for the current state of the project.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the reason for the current state of the project.")]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the start date for the project.
    /// </summary>
    /// <remarks>
    /// The date when work on the project is expected to start.
    /// </remarks>
    [Comment("Gets or sets the start date for the project.")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the target completion date for the project.
    /// </summary>
    [Comment("Gets or sets the target completion date for the project.")]
    public DateTime? TargetDate { get; set; }

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
    /// Gets or sets the effort required for the project.
    /// </summary>
    /// <remarks>
    /// Numeric value representing the effort (e.g., story points, hours).
    /// </remarks>
    [Comment("Gets or sets the effort required for the project.")]
    public int? Effort { get; set; }

    /// <summary>
    /// Gets or sets the business value of the project.
    /// </summary>
    /// <remarks>
    /// Numeric value representing the business value associated with completing the project.
    /// </remarks>
    [Comment("Gets or sets the business value of the project.")]
    public int? BusinessValue { get; set; }

    /// <summary>
    /// Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval.
    /// </summary>
    /// <remarks>
    /// This field is used to assign a rating to the project, which could be based on various factors such as priority, quality, or feedback from stakeholders. 
    /// The rating value is typically represented on a scale (e.g., 1 to 5) or a descriptive measure.
    /// </remarks>
    [Comment("Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval.")]
    public int? Rating { get; set; }
}