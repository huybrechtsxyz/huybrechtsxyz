using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a component or part of a design in a project. 
/// A project component can be hierarchical, allowing for nesting of subcomponents,
/// configurations, modules, and variants through recursion with the <see cref="SubComponents"/> property.
/// </summary>
/// <remarks>
/// This class allows for the creation of complex project designs where each component 
/// can have its own subcomponents, which could represent configurations, modules, or variants.
/// The hierarchical nature is enabled by the <see cref="SubComponents"/> property.
/// </remarks>
[MultiTenant]
[Table(nameof(ProjectComponent))]
[Comment("Represents a part of the design for a project.")]
[Index(nameof(TenantId), nameof(ProjectInfoId), nameof(ProjectDesignId), nameof(Sequence))]
[Index(nameof(TenantId), nameof(SearchIndex))]
public record ProjectComponent : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the ID of the project this component belongs to.
    /// </summary>
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the ID of the project design this component belongs to.
    /// </summary>
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project design for this component.
    /// </summary>
    /// <remarks>
    /// The design that this component is a part of, 
    /// representing its place within the overall project.
    /// </remarks>
    public ProjectDesign ProjectDesign { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the parent component, if any.
    /// </summary>
    /// <remarks>
    /// If this component is part of a larger component, 
    /// this property refers to the parent component.
    /// </remarks>
    public Ulid ParentId { get; set; } = Ulid.Empty;

    /// <summary>
    /// A list of subcomponents that belong to this component, allowing for hierarchical nesting.
    /// </summary>
    /// <remarks>
    /// Components can contain other components, forming a hierarchy where 
    /// each component can have multiple subcomponents.
    /// </remarks>
    public List<ProjectComponent> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the sequence order of this component within its parent design or component.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which components should be arranged or processed.
    /// </remarks>
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Gets or sets the name of the component.
    /// </summary>
    /// <remarks>
    /// Represents the human-readable name of the component, 
    /// such as "Kitchen" or "Dishwasher".
    /// </remarks>
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description for the component.
    /// </summary>
    /// <remarks>
    /// Provides additional details or context for what the component represents.
    /// </remarks>
    [MaxLength(256)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks or notes about the component.
    /// </summary>
    /// <remarks>
    /// This can include design notes, exceptions, or other relevant information.
    /// </remarks>
    public string? Remark { get; set; }

    /// <summary>
    /// A normalized search index to optimize searching through components.
    /// </summary>
    /// <remarks>
    /// This field concatenates normalized values to aid in fast lookups and queries.
    /// </remarks>
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Specifies the level of the component (e.g., Component, Configuration, Module, Variant).
    /// </summary>
    /// <remarks>
    /// Defines whether the component is a high-level entity (like a room), 
    /// a configuration option, or a smaller part (like a module or variant).
    /// </remarks>
    public ComponentLevel ComponentLevel { get; set; } = ComponentLevel.Component;

    /// <summary>
    /// Specifies the type of variant for this component (Standard, Option, Exceptional).
    /// </summary>
    /// <remarks>
    /// Defines whether this component is a standard part, an optional upgrade, 
    /// or an exceptional case for the design.
    /// </remarks>
    public VariantType VariantType { get; set; } = VariantType.Standard;

    /// <summary>
    /// Specifies the source type of this component (None, Platform).
    /// </summary>
    /// <remarks>
    /// This property determines if the component is sourced from a platform (like Azure) 
    /// or if it's custom-created without any platform association.
    /// </remarks>
    public SourceType SourceType { get; set; } = SourceType.None;

    /// <summary>
    /// Optional field to store the source of this component.
    /// </summary>
    /// <remarks>
    /// This can include external references or URLs that provide additional 
    /// context or details for the component.
    /// </remarks>
    public string? Source { get; set; }

    // PLATFORM RESOURCE

    /// <summary>
    /// Gets or sets the optional ID of the platform information associated with this component.
    /// </summary>
    public Ulid? PlatformInfoId { get; set; }

    /// <summary>
    /// Gets or sets the optional ID of the platform product associated with this component.
    /// </summary>
    public Ulid? PlatformProductId { get; set; }
}

/// <summary>
/// Defines the level of the component in the hierarchy.
/// </summary>
/// <remarks>
/// Components can be high-level items, configurations, modules, or variants.
/// </remarks>
public enum ComponentLevel
{
    /// <summary>
    /// A high-level component in the design, such as a room or a main section.
    /// </summary>
    Component,

    /// <summary>
    /// A specific configuration of a component, 
    /// such as a budget or premium version of a room.
    /// </summary>
    Configuration,

    /// <summary>
    /// A module that is part of a larger component, 
    /// such as the countertop in a kitchen design.
    /// </summary>
    Module,

    /// <summary>
    /// A variant of a module or configuration, 
    /// such as an upgraded appliance or feature.
    /// </summary>
    Variant
}

/// <summary>
/// Specifies the variant type of the component (Standard, Option, Exceptional).
/// </summary>
/// <remarks>
/// This defines whether the component is part of the standard design, 
/// an optional upgrade, or an exceptional case.
/// </remarks>
public enum VariantType
{
    /// <summary>
    /// The component is part of the standard design.
    /// </summary>
    Standard,

    /// <summary>
    /// The component is an optional upgrade that can be selected.
    /// </summary>
    Option,

    /// <summary>
    /// The component is an exceptional case, often used to handle edge cases in designs.
    /// </summary>
    Exceptional
}

/// <summary>
/// Defines the source type for the component (None, Platform).
/// </summary>
/// <remarks>
/// Specifies if the component is sourced from a platform 
/// (e.g., Azure resources) or if it’s not associated with any platform.
/// </remarks>
public enum SourceType
{
    /// <summary>
    /// No source is associated with the component.
    /// </summary>
    None = 0,

    /// <summary>
    /// The component is sourced from a platform, such as a cloud service.
    /// </summary>
    Platform = 2,
}
