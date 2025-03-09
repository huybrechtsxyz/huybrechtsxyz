using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core;

/// <summary>
/// Represents a base entity class implementing the <see cref="IEntity"/> interface.
/// </summary>
/// <remarks>
/// This class is marked as <see cref="MultiTenant"/>, indicating support for multiple tenants.
/// It provides common properties like <see cref="Id"/>, <see cref="TenantId"/>, <see cref="CreatedDT"/>, <see cref="ModifiedDT"/>, 
/// and <see cref="TimeStamp"/> for handling data consistency, tenant separation, and concurrency.
/// </remarks>
[MultiTenant]
public record Entity : IEntity
{
    /// <summary>
    /// Gets or sets the primary key for the entity.
    /// </summary>
    /// <value>
    /// The unique identifier for the entity, represented as a <see cref="Ulid"/>.
    /// </value>
    [Key]
    [Required]
    [Comment("Gets or sets the primary key for the entity.")]
    public Ulid Id { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    /// <value>
    /// A string representing the unique tenant identifier, which is required for multi-tenancy.
    /// </value>
    [Required]
    [MaxLength(64)]
    [Comment("Gets or sets the tenant identifier.")]
    public string TenantId { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Gets or sets the creation date and time for the entity.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing when the entity was created.
    /// </value>
    [Comment("Date time created")]
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
    [MaxLength(450)]
    [Comment("Gets or sets the ID of the user who created the entity.")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modified date and time for the entity.
    /// </summary>
    /// <value>
    /// A nullable <see cref="DateTime"/> that indicates when the entity was last modified, or null if not modified.
    /// </value>
    [Comment("Gets or sets the last modified date and time for the entity.")]
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
    [MaxLength(450)]
    [Comment("Gets or sets the ID of the user who last modified the entity.")]
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the concurrency timestamp for the entity.
    /// </summary>
    /// <value>
    /// A <see cref="Byte"/> array that is used to track concurrency conflicts.
    /// </value>
    [Timestamp]
    [Comment("Gets or sets the concurrency timestamp for the entity.")]
    public Byte[]? TimeStamp { get; set; }
}
