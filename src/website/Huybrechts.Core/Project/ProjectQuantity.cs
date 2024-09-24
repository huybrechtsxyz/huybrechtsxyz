using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Project;

/// <summary>
/// Represents a bill of quantities for a project, used to outline the materials and labor needed.
/// </summary>
/// <remarks>
/// The <c>ProjectQuantity</c> class serves as the header for a bill of quantities, which lists the materials, parts, and labor required to complete a project. 
/// Multiple bills of quantities can be created for a single project to reflect different scenarios or phases.
/// </remarks>
[MultiTenant]
[Table(nameof(ProjectQuantity))]
[Comment("Represents a bill of quantities for a project, detailing the materials, parts, and labor required.")]
[Index(nameof(TenantId), nameof(ProjectInfoId), nameof(Name), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
public record ProjectQuantity : Entity, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the project associated with this bill of quantities.
    /// </summary>
    /// <remarks>
    /// This is a foreign key linking to the <c>ProjectInfo</c> entity, indicating which project this bill of quantities is part of.
    /// </remarks>
    [Required]
    [Comment("Gets or sets the unique identifier for the project associated with this bill of quantities.")]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Gets or sets the associated project information.
    /// </summary>
    /// <remarks>
    /// This navigation property links to the <c>ProjectInfo</c> class, providing access to the project's details associated with this bill of quantities.
    /// </remarks>
    [Comment("Gets or sets the associated project information.")]
    public ProjectInfo ProjectInfo { get; set; } = new();

    /// <summary>
    /// Gets or sets the name of the bill of quantities.
    /// </summary>
    /// <remarks>
    /// The name provides a label or title for the bill of quantities, describing its purpose or scope.
    /// </remarks>
    [Required]
    [MaxLength(256)]
    [Comment("Gets or sets the name of the bill of quantities.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the bill of quantities.
    /// </summary>
    /// <remarks>
    /// This field provides a summary or explanation of what the bill of quantities covers, such as specific materials or work phases.
    /// </remarks>
    [MaxLength(1024)]
    [Comment("Gets or sets a description of the bill of quantities.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets any additional remarks for the bill of quantities.
    /// </summary>
    /// <remarks>
    /// This field can be used to store any extra notes or comments related to the bill of quantities.
    /// </remarks>
    [MaxLength(1024)]
    [Comment("Gets or sets any additional remarks for the bill of quantities.")]
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the tags associated with the bill of quantities.
    /// </summary>
    /// <remarks>
    /// Tags help categorize and search for bills of quantities by key attributes, such as phases or material types.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the tags associated with the bill of quantities.")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the search index for the bill of quantities.
    /// </summary>
    /// <remarks>
    /// This field is used to optimize lookups and queries based on specific attributes for the bill of quantities.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the search index for the bill of quantities.")]
    public string? SearchIndex { get; set; }
}
