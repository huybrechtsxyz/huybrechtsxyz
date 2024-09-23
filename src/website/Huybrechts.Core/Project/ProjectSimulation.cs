using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

// <summary>
/// Represents a simulation for a given project, containing various details and configurations related to the project's estimation.
/// </summary>
/// <remarks>
/// The <c>ProjectSimulation</c> class stores information about a simulation run for a particular project, allowing for different configurations, calculations, or estimates to be simulated.
/// </remarks>
[MultiTenant]
[Table(nameof(ProjectSimulation))]
[Comment("Represents a simulation for a given project, containing various details and configurations related to the project's estimation.")]
public record ProjectSimulation : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the project associated with this simulation.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectInfo</c> entity, indicating which project this simulation is part of.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the project associated with this simulation.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project information.
    /// </summary>
    /// <remarks>
    /// This navigation property links to the <c>ProjectInfo</c> class, providing access to the project's details associated with this simulation.
    /// </remarks>
    [Comment("Gets or sets the associated project information.")]
    public ProjectInfo ProjectInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the project simulation.
    /// </summary>
    /// <remarks>
    /// This field represents the name given to this specific simulation for identification and display purposes.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    [Comment("Gets or sets the name of the simulation.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the project simulation.
    /// </summary>
    /// <remarks>
    /// A detailed explanation of what this simulation entails, including any specific approaches, technologies, or considerations involved.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the description of the project simulation.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets if the project simulation is being calculated on this moment.
    /// </summary>
    /// <remarks>
    /// Is the background calculation of the project simulation active?
    /// </remarks>
    [Comment("Gets or sets if the project simulation is being calculated on this moment.")]
    public bool IsCalculating { get; set; } = false;

    /// <summary>
    /// Additional notes or remarks related to the project scenario.
    /// </summary>
    [Comment("Additional remarks or comments about the project simulation.")]
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the simulation.
    /// </summary>
    /// <remarks>
    /// Tags help categorize the simulation and improve searchability and filtering based on keywords.
    /// </remarks>
    [Comment("Keywords or categories for the simulation")]
    public string? Tags { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Navigation property to the simulation entries
    /// </summary>
    public virtual List<ProjectSimulationEntry> SimulationEntries { get; set; } = [];
}
