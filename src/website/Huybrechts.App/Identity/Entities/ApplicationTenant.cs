using Huybrechts.Core.Application;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.App.Identity.Entities;

[Table("ApplicationTenant")]
public sealed record ApplicationTenant : ApplicationTenantInfo
{
    public void UpdateFrom(ApplicationTenant entity)
    {
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Remark = entity.Remark;
    }
}