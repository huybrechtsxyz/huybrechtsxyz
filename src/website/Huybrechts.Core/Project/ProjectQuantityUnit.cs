using Finbuckle.MultiTenant;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents the unit details for a quantity entry in a Bill of Quantities.
/// This class allows linking a <c>ProjectComponent</c> and its unit with 
/// custom overrides for the unit price and quantities in a specific project.
/// </summary>
/// <remarks>
/// The <c>ProjectQuantityUnit</c> class handles overrides for unit pricing, 
/// quantity, and other details, providing flexibility in calculating 
/// costs based on different components and quantities.
/// </remarks>
[MultiTenant]
[Table("ProjectQuantityUnit")]
[Comment("The <c>ProjectQuantityUnit</c> class handles overrides for unit pricing and quantities")]
public record ProjectQuantityUnit : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectInfo</c> entity, indicating 
    /// which project this quantity unit is part of.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the project.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the Project Quantity.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectQuantity</c> entity.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the Project Quantity.")]
    public Ulid ProjectQuantityId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the ProjectQuantity linked to this unit.
    /// </summary>
    /// <remarks>
    /// The reference to the <c>ProjectQuantity</c> that this unit belongs to.
    /// </remarks>
    [ForeignKey(nameof(ProjectQuantityId))]
    [Comment("Gets or sets the ProjectQuantity linked to this unit.")]
    public ProjectQuantity ProjectQuantity { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique identifier for the Project Component.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectComponent</c> entity.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the Project Component.")]
    public Ulid ProjectComponentId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the Setup Unit.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>SetupUnit</c> entity.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the Setup Unit.")]
    public Ulid SetupUnitId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the Setup Unit linked to this unit.
    /// </summary>
    /// <remarks>
    /// The reference to the <c>SetupUnit</c> that this ProjectQuantityUnit belongs to.
    /// </remarks>
    [ForeignKey(nameof(SetupUnitId))]
    [Comment("Gets or sets the Setup Unit linked to this unit.")]
    public SetupUnit SetupUnit { get; set; } = new();

    /// <summary>
    /// Gets or sets the description of the unit or quantity entry.
    /// </summary>
    /// <remarks>
    /// Provides a detailed description of the unit or its usage in the project.
    /// </remarks>
    [StringLength(256)]
    [Comment("Gets or sets the description of the unit or quantity entry.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the unit or component.
    /// </summary>
    /// <remarks>
    /// Defines the category that this unit belongs to (e.g., labor, materials).
    /// </remarks>
    [Required]
    [StringLength(64)]
    [Comment("Gets or sets the category of the unit or component.")]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the unit used.
    /// </summary>
    /// <remarks>
    /// The total quantity of the unit for this specific entry.
    /// </remarks>
    [Required]
    [Precision(18, 6)]
    [Comment("Gets or sets the quantity of the unit used.")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the retail price per unit.
    /// </summary>
    /// <remarks>
    /// The price of the unit as sold to the customer (before discounts).
    /// </remarks>
    [Required]
    [Precision(18, 6)]
    [Comment("Gets or sets the retail price per unit.")]
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// Gets or sets the retail cost per unit.
    /// </summary>
    /// <remarks>
    /// The cost to the supplier or company for each unit (after discounts).
    /// </remarks>
    [Required]
    [Precision(18, 4)]
    [Comment("Gets or sets the retail cost per unit.")]
    public decimal RetailCost { get; set; }

    /// <summary>
    /// Gets or sets the final unit price after considering custom overrides.
    /// </summary>
    /// <remarks>
    /// The price per unit, which may be overridden from the standard pricing.
    /// </remarks>
    [Required]
    [Precision(18, 6)]
    [Comment("Gets or sets the final unit price after considering custom overrides.")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the final unit cost after considering custom overrides.
    /// </summary>
    /// <remarks>
    /// The cost per unit, which may be adjusted for this project.
    /// </remarks>
    [Required]
    [Precision(18, 6)]
    [Comment("Gets or sets the final unit cost after considering custom overrides.")]
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets any remarks associated with this unit entry.
    /// </summary>
    /// <remarks>
    /// Additional details or notes regarding the unit, such as special handling or conditions.
    /// </remarks>
    [Comment("Gets or sets any remarks associated with this unit entry.")]
    public string? Remark { get; set; }
}

