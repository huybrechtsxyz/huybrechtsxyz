using Finbuckle.MultiTenant;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a single entry in a project simulation, containing detailed information about project components, design, scenarios, and associated costs.
/// </summary>
[MultiTenant]
[Table(nameof(ProjectSimulationEntry))]
[Comment(" Represents a single entry in a project simulation")]
public record ProjectSimulationEntry : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the ProjectSimulation.
    /// </summary>
    /// <remarks>
    /// Links this unit to a specific project simulation.
    /// </remarks>
    [Required]
    public Ulid ProjectSimulationId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the project simulation.
    /// </summary>
    /// <remarks>
    /// The project simulation that this entry belongs to.
    /// </remarks>
    public ProjectSimulation ProjectSimulation { get; set; } = new();

    // <summary>
    /// Gets or sets the unique identifier for the project information.
    /// </summary>
    public Ulid ProjectInfoId { get; set; }

    public ProjectInfo ProjectInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the foreign key to the ProjectScenario.
    /// </summary>
    /// <remarks>
    /// Links this unit to a specific project scenario, allowing it to store metrics for that scenario.
    /// </remarks>
    [Required]
    public Ulid ProjectScenarioId { get; set; } = Ulid.Empty;

    public ProjectScenario ProjectScenario { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the project design this component belongs to.
    /// </summary>
    [Required]
    [Comment("Gets or sets the ID of the project design this component belongs to.")]
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    public ProjectDesign ProjectDesign { get; set; } = new();

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

    public ProjectComponent ProjectComponent { get; set; } = new();

    /// <summary>
    /// Gets or sets the unique identifier for the Project Quantity.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectQuantity</c> entity.
    /// </remarks>
    [Comment("Gets or sets the unique identifier for the Project Quantity.")]
    public Ulid? ProjectQuantityId { get; set; }

    public ProjectQuantity? ProjectQuantity { get; set; }

    ///// <summary>
    ///// Foreign key linking to the SetupUnit entity.
    ///// </summary>
    //[Required]
    //[Comment("Foreign key linking to the SetupUnit entity.")]
    //public Ulid? SetupUnitId { get; set; }

    //public SetupUnit? SetupUnit { get; set; }

    //
    // PLATFORM LINKS
    //

    /// <summary>
    /// Gets or sets the optional ID of the platform information associated with this component.
    /// </summary>
    [Comment("Gets or sets the optional ID of the platform information associated with this component.")]
    public Ulid? PlatformInfoId { get; set; }

    public PlatformInfo? PlatformInfo { get; set; }

    /// <summary>
    /// Gets or sets the optional ID of the platform product associated with this component.
    /// </summary>
    [Comment("Gets or sets the optional ID of the platform product associated with this component.")]
    public Ulid? PlatformProductId { get; set; }

    public PlatformProduct? PlatformProduct { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformRegion entity.
    /// Associates the rate with a specific region, reflecting geographical differences in pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformRegion entity.")]
    public Ulid PlatformRegionId { get; set; }

    public PlatformRegion? PlatformRegion { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformService entity.
    /// Links the rate to a specific service category, ensuring accurate pricing across products.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformService entity.")]
    public Ulid PlatformServiceId { get; set; }

    public PlatformService? PlatformService { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformRate table.
    /// This links the rate to a specific product offered on the platform, allowing for accurate pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformProduct entity.")]
    public Ulid PlatformRateId { get; set; } = Ulid.Empty;

    public PlatformRate? PlatformRate { get; set; }

    //
    // LINE DETAILS
    //

    /// <summary>
    /// Gets or sets the description of the unit.
    /// </summary>
    /// <remarks>
    /// Provides a detailed description of the unit or its usage in the project.
    /// </remarks>
    [StringLength(256)]
    [Comment("Gets or sets the description of the unit.")]
    public string? UnitDescription { get; set; }

    /// <summary>
    /// Category of the component unit (example: forfait, per unit, ...)
    /// </summary>
    [MaxLength(64)]
    [Comment("Category of the component unit (example: forfait, per unit, ...)")]
    public string? UnitCategory { get; set; }

    /// <summary>
    /// The currency code in which the rate is expressed.
    /// Follows standard ISO currency codes (e.g., USD, EUR) to ensure clarity in multi-currency environments.
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Comment("Currency code.")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// The unit of measure for the rate (e.g., per hour, per GB).
    /// Describes what the rate applies to, ensuring clarity in billing metrics.
    /// </summary>
    [MaxLength(64)]
    [Comment("Unit of measure.")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// The quantity of units for the service. 
    /// This represents the total number of service units being provided.
    /// </summary>
    /// <remarks>
    /// The quantity is stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Quantity of service units.")]
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
    /// The retail price of the service in the specified currency. 
    /// This is the total price customers will typically pay, including any applicable taxes or fees.
    /// </summary>
    /// <remarks>
    /// The retail price is stored with a precision of up to 6 decimal places, to accommodate variations in international currencies.
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Retail price in the specified currency.")]
    public decimal RetailPrice { get; set; }

    /// <summary>
    /// The unit price of the service in the specified currency. 
    /// This is the price per unit, allowing for more granular pricing control.
    /// </summary>
    /// <remarks>
    /// The unit price is stored with a precision of up to 6 decimal places.
    /// </remarks>
    [Precision(18, 6)]
    [Comment("Unit price per service unit in the specified currency.")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the percentage of ownership for this component.
    /// Represents the ownership share of the service or component. Default is 100% (full ownership).
    /// </summary>
    /// <remarks>
    /// Ownership percentage is an integer value. If less than 100, it indicates partial ownership.
    /// </remarks>
    [Comment("Percentage of ownership for this component or service.")]
    public int OwnershipPercentage { get; set; } = 100;

    /// <summary>
    /// The retail cost of the service in the specified currency. 
    /// This is the internal cost used for calculating profit margins.
    /// </summary>
    /// <remarks>
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Internal retail cost in the specified currency.")]
    public decimal SalesAmount { get; set; }

    /// <summary>
    /// The retail cost of the service in the specified currency. 
    /// This is the internal cost used for calculating profit margins.
    /// </summary>
    /// <remarks>
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Internal retail cost in the specified currency.")]
    public decimal RetailAmount { get; set; }

    /// <summary>
    /// The unit cost of the service in the specified currency. 
    /// This represents the internal cost per unit, used for cost breakdown and analysis.
    /// </summary>
    /// <remarks>
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Internal unit cost per service unit in the specified currency.")]
    public decimal UnitAmount { get; set; }

    /// <summary>
    /// The sales revenue adjusted for the ownership percentage. 
    /// </summary>
    /// <remarks>
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("The sales revenue adjusted for the ownership percentage.")]
    public decimal OwnSalesAmount { get; set; }

    /// <summary>
    /// The retail cost adjusted for the ownership percentage. 
    /// This is the retail cost that corresponds to the user's ownership share of the service.
    /// </summary>
    /// <remarks>
    /// Ownership percentage is factored into this retail cost.
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Ownership-adjusted retail cost in the specified currency.")]
    public decimal OwnRetailAmount { get; set; }

    /// <summary>
    /// The unit cost adjusted for the ownership percentage. 
    /// This is the unit cost that corresponds to the user's ownership share of the service.
    /// </summary>
    /// <remarks>
    /// Ownership percentage is factored into this unit cost.
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Ownership-adjusted unit cost per service unit in the specified currency.")]
    public decimal OwnUnitAmount{ get; set; }
}
