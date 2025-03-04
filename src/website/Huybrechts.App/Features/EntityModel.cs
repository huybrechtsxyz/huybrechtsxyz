using Huybrechts.Core;

namespace Huybrechts.App.Features.EntityFlow;

public record EntityModel : IEntity
{
    /// <summary>
    /// Gets or sets the primary key for the entity.
    /// </summary>
    /// <value>
    /// The unique identifier for the entity, represented as a <see cref="Ulid"/>.
    /// </value>
    public Ulid Id { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time for the entity.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing when the entity was created.
    /// </value>
    public DateTime CreatedDT { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the entity.
    /// </summary>
    /// <remarks>
    /// This field reflects the <see cref="ApplicationUser"/> who created the entity.
    /// </remarks>
    /// <value>
    /// An <c>nvarchar(450)</c> representing the unique identifier of the user.
    /// </value>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modified date and time for the entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="DateTime"/> that indicates when the entity was last modified, or null if not modified.
    /// </value>
    public DateTime? ModifiedDT { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified the entity.
    /// </summary>
    /// <remarks>
    /// This field reflects the <see cref="ApplicationUser"/> who last modified the entity.
    /// </remarks>
    /// <value>
    /// An <c>nvarchar(450)</c> representing the unique identifier of the user.
    /// </value>
    public string? ModifiedBy { get; set; }
}
