using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents different scenarios used to calculate project components and measures based on varying metrics.
/// </summary>
/// <remarks>
/// The <c>ProjectScenario</c> class includes details about a proposed metrics for a project. Each scenario can have multiple metrics and is used to find the right size for the project.
/// </remarks>
[MultiTenant]
[Table(nameof(ProjectScenario))]
[Comment("Represents different scenarios used to calculate design components and measures based on varying metrics.")]
[Index(nameof(TenantId), nameof(ProjectInfoId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
public record ProjectScenario : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the project associated with this scenario.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectInfo</c> entity, indicating which project this scenario is part of.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the project associated with this scenario.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project information.
    /// </summary>
    /// <remarks>
    /// This navigation property links to the <c>ProjectInfo</c> class, providing access to the project's details associated with this scenario.
    /// </remarks>
    [Comment("Gets or sets the associated project information.")]
    public ProjectInfo ProjectInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the project scenario.
    /// </summary>
    /// <remarks>
    /// This field represents the name given to this specific scenario for identification and display purposes.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    [Comment("Gets or sets the name of the scenario scenario.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the project scenario.
    /// </summary>
    /// <remarks>
    /// A detailed explanation of what this scenario entails, including any specific approaches, technologies, or considerations involved.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the description of the project scenario.")]
    public string? Description { get; set; }

    /// <summary>
    /// Additional notes or remarks related to the project scenario.
    /// </summary>
    [Comment("Additional remarks or comments about the project scenario.")]
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the scenario.
    /// </summary>
    /// <remarks>
    /// Tags help categorize the scenario and improve searchability and filtering based on keywords.
    /// </remarks>
    [Comment("Keywords or categories for the scenario")]
    public string? Tags { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Navigation to the scenario units
    /// </summary>
    public virtual List<ProjectScenarioUnit> Units { get; set; } = [];
}
