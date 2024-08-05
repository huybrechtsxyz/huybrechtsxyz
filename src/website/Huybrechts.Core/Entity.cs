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
    [DisplayName("ID")]
    [Comment("Primary Key")]
    public Ulid Id { get; set; }

    [DisplayName("Created date/time")]
    [Comment("Date time created")]
    public DateTime CreatedDT { get; set; }

    [DisplayName("Modified date/time")]
    [Comment("Modified time created")]
    public DateTime? ModifiedDT { get; set; }

    [Timestamp]
    public required Byte[] TimeStamp { get; set; }
}
