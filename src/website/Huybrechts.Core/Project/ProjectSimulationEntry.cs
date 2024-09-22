﻿using Huybrechts.Core.Platform;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a single entry in a project simulation, containing detailed information about project components, design, scenarios, and associated costs.
/// </summary>
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

    /// <summary>
    /// Gets or sets the foreign key to the ProjectScenario.
    /// </summary>
    /// <remarks>
    /// Links this unit to a specific project scenario, allowing it to store metrics for that scenario.
    /// </remarks>
    [Required]
    public Ulid ProjectScenarioId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the ID of the project design this component belongs to.
    /// </summary>
    [Required]
    [Comment("Gets or sets the ID of the project design this component belongs to.")]
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

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
    /// Foreign key linking to the SetupUnit entity.
    /// </summary>
    [Required]
    [Comment("Foreign key linking to the SetupUnit entity.")]
    public Ulid SetupUnitId { get; set; }

    //
    // PLATFORM LINKS
    //

    /// <summary>
    /// Gets or sets the optional ID of the platform information associated with this component.
    /// </summary>
    [Comment("Gets or sets the optional ID of the platform information associated with this component.")]
    public Ulid? PlatformInfoId { get; set; }

    /// <summary>
    /// Gets or sets the optional ID of the platform product associated with this component.
    /// </summary>
    [Comment("Gets or sets the optional ID of the platform product associated with this component.")]
    public Ulid? PlatformProductId { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformRegion entity.
    /// Associates the rate with a specific region, reflecting geographical differences in pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformRegion entity.")]
    public Ulid PlatformRegionId { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformService entity.
    /// Links the rate to a specific service category, ensuring accurate pricing across products.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformService entity.")]
    public Ulid PlatformServiceId { get; set; }

    /// <summary>
    /// Foreign key referencing the PlatformRate table.
    /// This links the rate to a specific product offered on the platform, allowing for accurate pricing.
    /// </summary>
    [Required]
    [Comment("Foreign key referencing the PlatformProduct entity.")]
    public Ulid PlatformRateId { get; set; } = Ulid.Empty;

    /// <summary>
    /// The currency code in which the rate is expressed.
    /// Follows standard ISO currency codes (e.g., USD, EUR) to ensure clarity in multi-currency environments.
    /// </summary>
    [Required]
    [MaxLength(10)]
    [Comment("Currency code.")]
    public string CurrencyCode { get; set; } = string.Empty;

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
    public decimal RetailCost { get; set; }

    /// <summary>
    /// The unit cost of the service in the specified currency. 
    /// This represents the internal cost per unit, used for cost breakdown and analysis.
    /// </summary>
    /// <remarks>
    /// Stored with a precision of up to 4 decimal places.
    /// </remarks>
    [Precision(18, 4)]
    [Comment("Internal unit cost per service unit in the specified currency.")]
    public decimal UnitCost { get; set; }

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
    public decimal OwnRetailCost { get; set; }

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
    public decimal OwnUnitCost { get; set; }

    public ProjectSimulationEntry() { }

    public ProjectSimulationEntry(
        ProjectSimulation simulation,
        ProjectInfo project,
        ProjectScenario scenario,
        ProjectDesign design,
        ProjectComponent component
        )
        : base()
    {
        Id = Ulid.NewUlid();
        CreatedDT = DateTime.Now;

        ProjectSimulation = simulation;
        ProjectSimulationId = simulation.Id;

        ProjectInfoId = project.Id;

        ProjectScenarioId = scenario.Id;

        ProjectDesignId = design.Id;

        ProjectComponentId = component.Id;
        OwnershipPercentage = component.OwnershipPercentage;
    }

    public ProjectSimulationEntry(
        ProjectSimulation simulation,
        ProjectInfo project,
        ProjectScenario scenario,
        ProjectDesign design,
        ProjectComponent component,
        PlatformInfo platform,
        PlatformProduct product,
        PlatformRate platformRate
        )
        : this(simulation, project, scenario, design, component)
    {
        PlatformInfoId = platform.Id;

        PlatformProductId = product.Id;

        PlatformRateId = platformRate.Id;
        PlatformRegionId = platformRate.PlatformRegionId;
        PlatformServiceId = platformRate.PlatformServiceId;
        CurrencyCode = platformRate.CurrencyCode;
        RetailPrice = platformRate.RetailPrice;
        UnitPrice = platformRate.UnitPrice;
    }

    public void Calculate()
    {
        this.RetailCost = decimal.Round(this.Quantity * this.RetailPrice, 4);
        this.UnitCost = decimal.Round(this.Quantity * this.UnitPrice, 4);
        this.OwnRetailCost = decimal.Round(this.RetailCost * (this.OwnershipPercentage / 100), 4);
        this.OwnUnitCost = decimal.Round(this.UnitCost * (this.OwnershipPercentage / 100), 4);
    }
}
