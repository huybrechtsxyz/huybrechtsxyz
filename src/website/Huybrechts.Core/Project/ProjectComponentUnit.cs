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
    /// Gets or sets the unique identifier for the Project Quantity.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectQuantity</c> entity.
    /// </remarks>
    [Comment("Gets or sets the unique identifier for the Project Quantity.")]
    public Ulid? ProjectQuantityId { get; set; }

    /// <summary>
    /// Gets or sets the ProjectQuantity linked to this unit.
    /// </summary>
    /// <remarks>
    /// The reference to the <c>ProjectQuantity</c> that this unit belongs to.
    /// </remarks>
    [ForeignKey(nameof(ProjectQuantityId))]
    [Comment("Gets or sets the ProjectQuantity linked to this unit.")]
    public ProjectQuantity? ProjectQuantity { get; set; }

    /// <summary>
    /// Gets or sets the sequence order of this component within its parent design or component unit.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which component units should be arranged or processed.
    /// </remarks>
    [Comment("Gets or sets the sequence order of this component within its parent design or component unit.")]
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Gets or sets the description of the unit.
    /// </summary>
    /// <remarks>
    /// Provides a detailed description of the unit or its usage in the project.
    /// </remarks>
    [StringLength(256)]
    [Comment("Gets or sets the description of the unit.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the name of the variable used in calculations.
    /// </summary>
    /// <remarks>
    /// This represents the variable name used in formulas for calculating the component's cost.
    /// </remarks>
    [MaxLength(128)]
    [Comment("The variable name used in the metrics calculations for this component unit.")]
    public string? Variable { get; set; }

    /// <summary>
    /// Category of the component unit (example: forfait, per unit, ...)
    /// </summary>
    [MaxLength(64)]
    [Comment("Category of the component unit (example: forfait, per unit, ...)")]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the formula expression used to calculate the quantity value.
    /// </summary>
    /// <remarks>
    /// This formula will be applied to derive the cost or value of the component based on input metrics.
    /// </remarks>
    [MaxLength(256)]
    [Comment("The formula used to calculate the value of the quantity for this component unit.")]
    public string? Expression { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the unit used.
    /// </summary>
    /// <remarks>
    /// The total quantity of the unit for this specific entry.
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Gets or sets the quantity of the unit used.")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the sales price for a service.
    /// </summary>
    /// <remarks>
    /// The price of the unit as sold to the customer
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Gets or sets the sales price for a unit.")]
    public decimal SalesPrice { get; set; }

    /// <summary>
    /// Gets or sets the standard price for a service.
    /// </summary>
    /// <remarks>
    /// The price of the unit as sold to the customer
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Gets or sets the standard price for a unit.")]
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// Gets or sets the actual price you pay for a unit.
    /// </summary>
    /// <remarks>
    /// The price per unit.
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Gets or sets the actual price you pay for a unit.")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks or notes about the component unit.
    /// </summary>
    /// <remarks>
    /// This can include design notes, exceptions, or other relevant information.
    /// </remarks>
    [Comment("Gets or sets any additional remarks or notes about the component unit.")]
    public string? Remark { get; set; }
}
