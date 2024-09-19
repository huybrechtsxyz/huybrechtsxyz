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
    [Required]
    [Comment("Gets or sets the ID of the project this component belongs to.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the ID of the project design this component belongs to.
    /// </summary>
    [Required]
    [Comment("Gets or sets the ID of the project design this component belongs to.")]
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project design for this component.
    /// </summary>
    /// <remarks>
    /// The design that this component is a part of, 
    /// representing its place within the overall project.
    /// </remarks>
    [Comment("Gets or sets the associated project design for this component.")]
    public ProjectDesign ProjectDesign { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the parent component, if any.
    /// </summary>
    /// <remarks>
    /// If this component is part of a larger component, 
    /// this property refers to the parent component.
    /// </remarks>
    [Comment("Gets or sets the ID of the parent component, if any.")]
    public Ulid ParentId { get; set; } = Ulid.Empty;

    /// <summary>
    /// A list of subcomponents that belong to this component, allowing for hierarchical nesting.
    /// </summary>
    /// <remarks>
    /// Components can contain other components, forming a hierarchy where 
    /// each component can have multiple subcomponents.
    /// </remarks>
    [Comment("A list of subcomponents that belong to this component, allowing for hierarchical nesting.")]
    public List<ProjectComponent> Children { get; set; } = [];

    // navigation to unitss
    public List<ProjectComponentUnit> ProjectComponentUnits { get; set; } = [];

    /// <summary>
    /// Gets or sets the sequence order of this component within its parent design or component.
    /// </summary>
    /// <remarks>
    /// Used to determine the order in which components should be arranged or processed.
    /// </remarks>
    [Comment("Gets or sets the sequence order of this component within its parent design or component.")]
    public int Sequence { get; set; } = 0;

    /// <summary>
    /// Gets or sets the name of the component.
    /// </summary>
    /// <remarks>
    /// Represents the human-readable name of the component, 
    /// such as "Kitchen" or "Dishwasher".
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the name of the component.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description for the component.
    /// </summary>
    /// <remarks>
    /// Provides additional details or context for what the component represents.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the description of the component.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks or notes about the component.
    /// </summary>
    /// <remarks>
    /// This can include design notes, exceptions, or other relevant information.
    /// </remarks>
    [Comment("Gets or sets any additional remarks or notes about the component.")]
    public string? Remark { get; set; }

    /// <summary>
    /// A normalized search index to optimize searching through components.
    /// </summary>
    /// <remarks>
    /// This field concatenates normalized values to aid in fast lookups and queries.
    /// </remarks>
    [Comment("A normalized search index to optimize searching through components.")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Specifies the level of the component (e.g., Component, Configuration, Module, Variant).
    /// </summary>
    /// <remarks>
    /// Defines whether the component is a high-level entity (like a room), 
    /// a configuration option, or a smaller part (like a module or variant).
    /// </remarks>
    [Comment("Specifies the level of the component (e.g., Component, Configuration, Module, Variant).")]
    public ComponentLevel ComponentLevel { get; set; } = ComponentLevel.Component;

    /// <summary>
    /// Specifies the type of variant for this component (Standard, Option, Exceptional).
    /// </summary>
    /// <remarks>
    /// Defines whether this component is a standard part, an optional upgrade, 
    /// or an exceptional case for the design.
    /// </remarks>
    [Comment("Specifies the type of variant for this component (Standard, Option, Exceptional).")]
    public VariantType VariantType { get; set; } = VariantType.Standard;

    /// <summary>
    /// Gets or sets the proposal associated with the component.
    /// </summary>
    /// <remarks>
    /// Represents the proposal or solution being suggested for this component.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the proposal associated with the component.")]
    public string? Proposal { get; set; }

    /// <summary>
    /// Gets or sets the account under which this component is managed.
    /// </summary>
    /// <remarks>
    /// The account represents the platform or service under which the component is linked.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the account under which this component is managed.")]
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the account under which this component is managed.
    /// </summary>
    /// <remarks>
    /// The Organization represents the platform or service under which the component is linked.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the organization under which this component is managed.")]
    public string? Organization { get; set; }

    /// <summary>
    /// Gets or sets the account under which this component is managed.
    /// </summary>
    /// <remarks>
    /// The subscription represents the platform or service under which the component is linked.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the account under which this component is managed.")]
    public string? OrganizationalUnit { get; set; }

    /// <summary>
    /// Gets or sets the location associated with this component.
    /// </summary>
    /// <remarks>
    /// This field specifies the physical or logical location relevant to the component.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the location associated with the component.")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the group to which this component belongs.
    /// </summary>
    /// <remarks>
    /// This is used to categorize the component into different groups, making it easier to organize and manage.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the group to which this component belongs.")]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the environment associated with this component.
    /// </summary>
    /// <remarks>
    /// This field specifies the environment relevant to the component.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the environment associated with the component.")]
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the responsible to which this component belongs.
    /// </summary>
    /// <remarks>
    /// This field specifies whop is responsible for the component.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the responsible to which this component belongs.")]
    public string? Responsible { get; set; }

    /// <summary>
    /// Gets or sets the percentage of ownership for this component.
    /// </summary>
    /// <remarks>
    /// Indicates the ownership share of this component, default is 100%.
    /// </remarks>
    [Comment("Gets or sets the percentage of ownership for this component.")]
    public int OwnershipPercentage { get; set; } = 100;

    /// <summary>
    /// Specifies the source type of this component (None, Platform).
    /// </summary>
    /// <remarks>
    /// This property determines if the component is sourced from a platform (like Azure) 
    /// or if it's custom-created without any platform association.
    /// </remarks>
    [Comment("Specifies the source type of this component (None, Platform).")]
    public SourceType SourceType { get; set; } = SourceType.None;

    /// <summary>
    /// Optional field to store the source of this component.
    /// </summary>
    /// <remarks>
    /// This can include external references or URLs that provide additional 
    /// context or details for the component.
    /// </remarks>
    [Comment("Optional field to store the source of this component.")]
    public string? Source { get; set; }

    // PLATFORM RESOURCE

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
    [Display(Name = nameof(ComponentLevel) + "_" + nameof(Component), Description = nameof(ComponentLevel) + "_" + nameof(Component) + "_d", ResourceType = typeof(Localization))]
    [Comment("A high-level component in the design, such as a room or a main section.")]
    Component = 1,

    /// <summary>
    /// A specific configuration of a component, such as a budget or premium version of a room.
    /// </summary>
    [Display(Name = nameof(ComponentLevel) + "_" + nameof(Configuration), Description = nameof(ComponentLevel) + "_" + nameof(Configuration) + "_d", ResourceType = typeof(Localization))]
    [Comment("A specific configuration of a component, such as a budget or premium version of a room.")]
    Configuration,

    /// <summary>
    /// A module that is part of a larger component, such as the countertop in a kitchen design.
    /// </summary>
    [Display(Name = nameof(ComponentLevel) + "_" + nameof(Module), Description = nameof(ComponentLevel) + "_" + nameof(Module) + "_d", ResourceType = typeof(Localization))]
    [Comment("A module that is part of a larger component, such as the countertop in a kitchen design.")]
    Module,

    /// <summary>
    /// A variant of a module or configuration, such as an upgraded appliance or feature.
    /// </summary>
    [Display(Name = nameof(ComponentLevel) + "_" + nameof(Variant), Description = nameof(ComponentLevel) + "_" + nameof(Variant) + "_d", ResourceType = typeof(Localization))]
    [Comment("A variant of a module or configuration, such as an upgraded appliance or feature.")]
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
    [Display(Name = nameof(VariantType) + "_" + nameof(Standard), Description = nameof(VariantType) + "_" + nameof(Standard) + "_d", ResourceType = typeof(Localization))]
    [Comment("The component is part of the standard design.")]
    Standard = 1,

    /// <summary>
    /// The component is an optional upgrade that can be selected.
    /// </summary>
    [Display(Name = nameof(VariantType) + "_" + nameof(Option), Description = nameof(VariantType) + "_" + nameof(Option) + "_d", ResourceType = typeof(Localization))]
    [Comment("The component is an optional upgrade that can be selected.")]
    Option,

    /// <summary>
    /// The component is an exceptional case, often used to handle edge cases in designs.
    /// </summary>
    [Display(Name = nameof(VariantType) + "_" + nameof(Exceptional), Description = nameof(VariantType) + "_" + nameof(Exceptional) + "_d", ResourceType = typeof(Localization))]
    [Comment("The component is an exceptional case, often used to handle edge cases in designs.")]
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
    [Display(Name = nameof(SourceType) + "_" + nameof(None), Description = nameof(SourceType) + "_" + nameof(None) + "_d", ResourceType = typeof(Localization))]
    [Comment("No source is associated with the component.")]
    None = 0,

    /// <summary>
    /// The component is sourced from a platform, such as a cloud service.
    /// </summary>
    [Display(Name = nameof(SourceType) + "_" + nameof(Platform), Description = nameof(SourceType) + "_" + nameof(Platform) + "_d", ResourceType = typeof(Localization))]
    [Comment("The component is part of the standard design.")]
    Platform = 2,
}
