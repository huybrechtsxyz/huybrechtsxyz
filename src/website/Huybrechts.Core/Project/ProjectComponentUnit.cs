using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Huybrechts.Core.Setup;

namespace Huybrechts.Core.Project;

/// <summary>
/// Links a project component to a measuring unit, allowing for cost calculation using metrics.
/// </summary>
[MultiTenant]
[Table(nameof(ProjectComponentUnit))]
[Comment("Links a project component to a measuring unit, allowing for cost calculation using metrics.")]
public record ProjectComponentUnit : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the ID of the associated project.
    /// </summary>
    [Required]
    [Comment("The project ID this component unit is part of.")]
    public Ulid ProjectInfoId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated project design.
    /// </summary>
    [Required]
    [Comment("The design ID of the project this component unit belongs to.")]
    public Ulid ProjectDesignId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated project component.
    /// </summary>
    /// <remarks>
    /// This field links the unit to a specific component within the project design.
    /// </remarks>
    [Required]
    [ForeignKey(nameof(ProjectComponent))]
    [Comment("The ID of the project component to which this unit belongs.")]
    public Ulid ProjectComponentId { get; set; }

    /// <summary>
    /// Navigation property to the related ProjectComponent.
    /// </summary>
    public ProjectComponent ProjectComponent { get; set; } = new();

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
    /// Gets or sets the sequence order of this component within its parent design or component unit.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which component units should be arranged or processed.
    /// </remarks>
    [Comment("Gets or sets the sequence order of this component within its parent design or component unit.")]
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Gets or sets the name of the variable used in calculations.
    /// </summary>
    /// <remarks>
    /// This represents the variable name used in formulas for calculating the component's cost.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    [Comment("The variable name used in the metrics calculations for this component unit.")]
    public string Variable { get; set; } = string.Empty;

    /// <summary>
    /// Category of the component unit (example: forfait, per unit, ...)
    /// </summary>
    [MaxLength(64)]
    [Comment("Category of the component unit (example: forfait, per unit, ...)")]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the formula expression used to calculate the variable's value.
    /// </summary>
    /// <remarks>
    /// This formula will be applied to derive the cost or value of the component based on input metrics.
    /// </remarks>
    [MaxLength(256)]
    [Comment("The formula used to calculate the value of the variable for this component unit.")]
    public string? Expression { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks or notes about the component unit.
    /// </summary>
    /// <remarks>
    /// This can include design notes, exceptions, or other relevant information.
    /// </remarks>
    [Comment("Gets or sets any additional remarks or notes about the component unit.")]
    public string? Remark { get; set; }
}
