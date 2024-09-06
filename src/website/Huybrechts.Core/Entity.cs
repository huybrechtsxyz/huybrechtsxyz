using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core;

public interface IEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    Ulid Id { get; set; }
}

[MultiTenant]
public record Entity : IEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    [Required]
    [Comment("Primary Key")]
    public Ulid Id { get; set; }

    [Required]
    [MaxLength(64)]
    [Comment("The tenant identifier")]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Date time created
    /// </summary>
    [Comment("Date time created")]
    public DateTime CreatedDT { get; set; }

    /// <summary>
    /// Modified time created
    /// </summary>
    [Comment("Modified time created")]
    public DateTime? ModifiedDT { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    [Timestamp]
    public Byte[]? TimeStamp { get; set; }
}
