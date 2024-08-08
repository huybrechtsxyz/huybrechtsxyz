using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// The platform that can be used in project
/// </summary>
[MultiTenant]
[Table("Platform")]
[Comment("Platforms that provide compute resources")]
public record PlatformInfo : Entity, IEntity
{
    /// <summary>
    /// Name
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the platform")]
    public string Name { get; set;} = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    [MaxLength(256)]
    [Comment("Description of the platform")]
    public string? Description { get; set;}

    /// <summary>
    /// Supported provider for automation purposes
    /// </summary>
    [Comment("Supported automation providers of the platform")]
    public PlatformProvider Provider { get; set; }

    /// <summary>
    /// General remark
    /// </summary>
    [Comment("Remark about the platform")]
    public string? Remark { get; set; }
}

/// <summary>
/// Providers that are support for automation
/// </summary>
public enum PlatformProvider
{
    /// <summary>
    /// No automation provider
    /// </summary>
    [Display(Name = nameof(PlatformProvider) + "_" + nameof(None), Description = nameof(PlatformProvider) + "_" + nameof(None) + "_d", ResourceType = typeof(Localization))]
    [Comment("No automation provider")]
    None = 0,

    /// <summary>
    /// Azure automation provider
    /// </summary>
    [Display(Name = nameof(PlatformProvider) + "_" + nameof(Azure), Description = nameof(PlatformProvider) + "_" + nameof(Azure) + "_d", ResourceType = typeof(Localization))]
    [Comment("Azure as automation provider")]
    Azure = 1
}