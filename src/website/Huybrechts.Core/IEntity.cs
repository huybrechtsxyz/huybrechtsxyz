namespace Huybrechts.Core;

/// <summary>
/// Represents an entity with a unique identifier.
/// </summary>
/// <remarks>
/// This interface enforces the presence of a primary key (Id) for any implementing class.
/// The Id is of type <see cref="Ulid"/>, which is a universally unique lexicographically sortable identifier.
/// It can be useful in distributed systems or databases where a unique key is required.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the primary key for the entity.
    /// </summary>
    /// <value>
    /// The unique identifier for the entity, represented as a <see cref="Ulid"/>.
    /// </value>
    /// <remarks>
    /// The Id should be set upon creation of the entity and remain immutable to ensure consistency.
    /// </remarks>
    Ulid Id { get; set; }
}
