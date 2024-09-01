using Finbuckle.MultiTenant;
using Huybrechts.Core.Platform;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Setup;

/// <summary>
/// Represents a custom state that can be applied to various objects, such as projects, constraints, requirements, and more.
/// </summary>
/// <remarks>
/// This class allows users to define custom states, including their types and associated metadata, for better tracking and management.
/// </remarks>
[MultiTenant]
[Table("SetupState")]
[Index(nameof(ObjectType),nameof(Name), IsUnique = true)]
[Index(nameof(SearchIndex))]
[Comment("Represents a custom state that can be applied to various objects, such as projects, constraints, requirements, and more.")]
public record SetupState : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the type of object to which this state applies.
    /// </summary>
    /// <remarks>
    /// This enum indicates the object type (e.g., Project, Constraint, Requirement) that the state is associated with.
    /// </remarks>
    [Required]
    public ObjectType ObjectType { get; set; }

    /// <summary>
    /// Gets or sets the type of the state.
    /// </summary>
    /// <remarks>
    /// The StateType enum indicates the current status of the object, such as New, Proposed, Active, Resolved, or Done.
    /// </remarks>
    [Required]
    public StateType StateType { get; set; }


    /// <summary>
    /// Gets or sets the name of the state.
    /// </summary>
    /// <remarks>
    /// The Name provides a human-readable identifier for the state, useful in user interfaces and reports.
    /// </remarks>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequence number for the state.
    /// </summary>
    /// <remarks>
    /// The Sequence determines the display order of the states. Lower numbers are displayed before higher numbers.
    /// </remarks>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets a description for the state.
    /// </summary>
    /// <remarks>
    /// The Description provides detailed information about the state, including its purpose and when it should be used.
    /// </remarks>
    [MaxLength(256)]
    public string? Description { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}

/// <summary>
/// Defines the types of objects that can have custom states.
/// </summary>
public enum ObjectType
{
    /// <summary>
    /// Represents a project within the platform.
    /// </summary>
    [Display(Name = nameof(ObjectType) + "_" + nameof(Project), Description = nameof(ObjectType) + "_" + nameof(Project) + "_d", ResourceType = typeof(Localization))]
    [Comment("Represents a project within the platform.")]
    Project = 1
}

/// <summary>
/// Defines the various state types that can be assigned to objects within the platform.
/// </summary>
/// <remarks>
/// This enumeration is used to categorize the lifecycle stages of different entities, such as projects, tasks, requirements, and more.
/// Each state type represents a specific phase or condition an object can be in, allowing for better tracking and management of its progress.
/// </remarks>
public enum StateType
{
    /// <summary>
    /// Indicates that the object is newly created and has not yet been processed or reviewed.
    /// </summary>
    /// <remarks>
    /// This state is typically assigned when an object is first introduced to the platform. 
    /// It signifies that the object is awaiting further action or evaluation.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(New), Description = nameof(StateType) + "_" + nameof(New) + "_d", ResourceType = typeof(Localization))]
    [Comment("Represents a project within the platform.")]
    New = 1,

    /// <summary>
    /// Represents a state where the object has been suggested or put forward for consideration.
    /// </summary>
    /// <remarks>
    /// An object in this state is under review or being evaluated for its relevance or feasibility.
    /// It may require additional input or approval before moving forward.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(Proposed), Description = nameof(StateType) + "_" + nameof(Proposed) + "_d", ResourceType = typeof(Localization))]
    [Comment("Represents a state where the object has been suggested or put forward for consideration.")]
    Proposed,

    /// <summary>
    /// Denotes that the object is currently in active development, implementation, or execution.
    /// </summary>
    /// <remarks>
    /// This state is used for objects that are being actively worked on. 
    /// It indicates that the necessary resources have been allocated and work is underway.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(InProgress), Description = nameof(StateType) + "_" + nameof(InProgress) + "_d", ResourceType = typeof(Localization))]
    [Comment("Denotes that the object is currently in active development, implementation, or execution.")]
    InProgress,

    /// <summary>
    /// Signifies that the object has been resolved or a solution has been found.
    /// </summary>
    /// <remarks>
    /// For issues or tasks, this state means that the work is complete and the object is no longer in a problematic state. 
    /// For other objects, it might mean that the objectives have been met or the desired outcomes achieved.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(Resolved), Description = nameof(StateType) + "_" + nameof(Resolved) + "_d", ResourceType = typeof(Localization))]
    [Comment("Signifies that the object has been resolved or a solution has been found.")]
    Resolved,

    /// <summary>
    /// Marks that the object has been fully completed and all associated activities have been finalized.
    /// </summary>
    /// <remarks>
    /// This state indicates that no further actions are required for the object. 
    /// It is considered finished and is now in a state of closure.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(Completed), Description = nameof(StateType) + "_" + nameof(Completed) + "_d", ResourceType = typeof(Localization))]
    [Comment("Marks that the object has been fully completed.")]
    Completed,

    /// <summary>
    /// Indicates that the object has been removed from the active list, often due to irrelevance, redundancy, or obsolescence.
    /// </summary>
    /// <remarks>
    /// This state is assigned when an object is no longer needed and has been intentionally removed. 
    /// It could be due to changes in scope, priorities, or completion criteria.
    /// </remarks>
    [Display(Name = nameof(StateType) + "_" + nameof(Removed), Description = nameof(StateType) + "_" + nameof(Removed) + "_d", ResourceType = typeof(Localization))]
    [Comment("Indicates that the object has been removed from the active list.")]
    Removed
}
