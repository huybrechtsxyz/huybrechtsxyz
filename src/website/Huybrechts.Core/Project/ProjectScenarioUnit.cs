using Finbuckle.MultiTenant;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a unit of measurement or metric used in a project scenario for calculating values over design components.
/// </summary>
[MultiTenant]
[Table("ProjectScenarioUnit")]
[Comment("Represents a unit of measurement or metric used in a project scenario for calculating values over design components.")]
public record ProjectScenarioUnit: Entity, IEntity
{
    /// <summary>
    /// Gets or sets the ID of the project this scenario metric belongs to.
    /// </summary>
    [Required]
    [Comment("Gets or sets the ID of the project this scenario metric belongs to.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the foreign key to the ProjectScenario.
    /// </summary>
    /// <remarks>
    /// Links this unit to a specific project scenario, allowing it to store metrics for that scenario.
    /// </remarks>
    [Required]
    public Ulid ProjectScenarioId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the project scenario.
    /// </summary>
    /// <remarks>
    /// The project scenario that this unit belongs to. Defines the context in which this metric is used.
    /// </remarks>
    public ProjectScenario ProjectScenario { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the measuring unit.
    /// </summary>
    /// <remarks>
    /// Links the component to the measuring unit that will be used for metrics and calculations.
    /// </remarks>
    [ForeignKey(nameof(SetupUnit))]
    [Comment("The ID of the measuring unit used for this component.")]
    public Ulid? SetupUnitId { get; set; }

    /// <summary>
    /// Gets or sets the associated setup measuring unit.
    /// </summary>
    [Comment("The setup measuring unit that this component unit is tied to.")]
    public SetupUnit? SetupUnit { get; set; } = new();

    /// <summary>
    /// Gets or sets the sequence order of this unit within its parent scenario.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which scenario metric should be arranged or processed.
    /// </remarks>
    [Comment("Gets or sets the sequence order of this unit within its parent scenario.")]
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Gets or sets the name of the metric variable.
    /// </summary>
    /// <remarks>
    /// Represents the variable name used to identify the metric. For example, "Length" or "RequestsPerSecond".
    /// </remarks>
    [Required]
    [MaxLength(128)]
    public string Variable { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an expression that calculates the metric dynamically.
    /// </summary>
    /// <remarks>
    /// This allows a dynamic calculation of the variable based on a formula, such as `Length * Width` for area or a more complex business rule.
    /// </remarks>
    [MaxLength(512)]
    public string? Expression { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks or notes about the scenario unit.
    /// </summary>
    /// <remarks>
    /// This can include design notes, exceptions, or other relevant information.
    /// </remarks>
    [Comment("Gets or sets any additional remarks or notes about the scenario unit.")]
    public string? Remark { get; set; }
}
