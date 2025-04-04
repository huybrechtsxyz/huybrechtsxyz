﻿using Finbuckle.MultiTenant;
using FluentResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Application;

[Table("ApplicationTenant")]
public record ApplicationTenant
{
    [Key]
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    public string Id { get; set; } = string.Empty;

    public ApplicationTenantState State { get; set; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public string? Remark { get; set; }

    public byte[]? Picture { get; set; }

    [StringLength(32)]
    public string? DatabaseProvider { get; set; } = string.Empty;

    [StringLength(512)]
    public string? ConnectionString { get; set; }

    [Timestamp]
    public byte[]? ConcurrencyStamp { get; set; }

    public void UpdateFrom(ApplicationTenant entity)
    {
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Remark = entity.Remark;
        if (entity.Picture is not null)
        {
            this.Picture = new byte[entity.Picture.Length];
            Array.Copy(entity.Picture, this.Picture, entity.Picture.Length);
        }
    }

    public TenantInfo ToTenantInfo()
    {
        return new TenantInfo()
        {
            Identifier = Id,
            Id = Id,
            Name = Name
        };
    }
}
