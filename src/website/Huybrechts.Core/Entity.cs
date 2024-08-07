using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core;

public interface IEntity
{
    Ulid Id { get; set; }
}

[MultiTenant]
public record Entity : IEntity
{
    [Key]
    [Required]
    [Comment("Primary Key")]
    public Ulid Id { get; set; }

    [Comment("Date time created")]
    public DateTime CreatedDT { get; set; }

    [Comment("Modified time created")]
    public DateTime? ModifiedDT { get; set; }

    [Timestamp]
    public Byte[] TimeStamp { get; set; } = default!;
}
