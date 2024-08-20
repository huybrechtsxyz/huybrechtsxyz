using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Platform;

/// <summary>
/// Represents a cloud platform or on-premise hosting solution that can be used within the project.
/// This entity encapsulates the platform's essential details, including its name, description, and supported automation provider.
/// </summary>
[MultiTenant]
[Table("Platform")]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(SearchIndex))]
[Comment("Table storing information about platforms that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.")]
public record PlatformInfo : Entity, IEntity
{
    /// <summary>
    /// The name of the platform (e.g., Azure, Google Cloud, On-Premise).
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Comment("Name of the platform.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A brief description of the platform, detailing its purpose or features.
    /// </summary>
    [MaxLength(256)]
    [Comment("Detailed description of the platform.")]
    public string? Description { get; set; }

    /// <summary>
    /// Specifies the automation provider that the platform supports, if any.
    /// </summary>
    [Comment("The platform's supported automation provider, enabling automated resource management.")]
    public PlatformProvider Provider { get; set; }

    /// <summary>
    /// Additional notes or remarks related to the platform.
    /// </summary>
    [Comment("Additional remarks or comments about the platform.")]
    public string? Remark { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }
}

/// <summary>
/// Enum representing the automation providers supported by a platform.
/// This defines whether a platform supports automation and, if so, which provider is available.
/// </summary>
public enum PlatformProvider
{
    // <summary>
    /// Indicates that no automation provider is supported.
    /// </summary>
    [Display(Name = nameof(PlatformProvider) + "_" + nameof(None), Description = nameof(PlatformProvider) + "_" + nameof(None) + "_d", ResourceType = typeof(Localization))]
    [Comment("No automation provider available.")]
    None = 0,

    /// <summary>
    /// Indicates that Azure is the supported automation provider.
    /// </summary>
    [Display(Name = nameof(PlatformProvider) + "_" + nameof(Azure), Description = nameof(PlatformProvider) + "_" + nameof(Azure) + "_d", ResourceType = typeof(Localization))]
    [Comment("Azure is supported as an automation provider.")]
    Azure = 1
}