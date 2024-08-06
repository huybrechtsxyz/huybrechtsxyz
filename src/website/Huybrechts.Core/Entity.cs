using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Core;

public interface IEntity
{
    Ulid Id { get; set; }
}

public interface ICopyableEntity
{
    void CopyFrom(IEntity entity);
}

[MultiTenant]
public record Entity : IEntity, ICopyableEntity
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
    public Byte[] TimeStamp { get; set; } = default!;

    public void Initialize()
    {
        Id = new Ulid();
        CreatedDT = DateTime.UtcNow;
    }

    public virtual void CopyFrom(IEntity entity)
    {
        throw new NotImplementedException();
    }
}
