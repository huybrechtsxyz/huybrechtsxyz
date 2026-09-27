using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huybrechts.Core.Wiki;

/// <summary>
/// Represents a wiki page that is associated with a tenant and project, 
/// using Finbuckle.MultiTenant for multi-tenancy support.
/// </summary>
/// <remarks>
/// This class handles the structure and metadata for wiki pages. Each page can belong 
/// to a tenant or a project and includes various attributes such as namespace, path, 
/// and markdown content. The <c>NamespaceType</c> is used to differentiate between 
/// tenant-wide and project-specific wikis.
/// </remarks>
[MultiTenant]
[Table("WikiPage")]
[Index(nameof(TenantId), nameof(Namespace), nameof(Page), IsUnique = true)]
[Index(nameof(TenantId), nameof(SearchIndex))]
[Comment("Represents a wiki page.")]
public record WikiPage: Entity, IEntity
{
    /// <summary>
    /// Gets or sets the namespace to which the wiki page belongs (e.g., 'UserGuide').
    /// </summary>
    /// <remarks>
    /// This represents the category or group the page is part of, such as 'UserGuide', 'Admin', etc.
    /// </remarks>
    [MaxLength(128)]
    [Comment("Gets or sets the namespace to which the wiki page belongs (e.g., 'UserGuide').")]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path or URL slug for the wiki page.
    /// </summary>
    /// <remarks>
    /// The path defines how the page can be accessed through the web, for example, 'how/to/catch/path'.
    /// </remarks>
    [MaxLength(512)]
    [Comment("Gets or sets the page or URL slug for the wiki page.")]
    public string Page { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the wiki page.
    /// </summary>
    /// <remarks>
    /// The title is a short, descriptive name for the content of the page.
    /// </remarks>
    [MaxLength(512)]
    [Comment("Gets or sets the title of the wiki page.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the wiki.
    /// </summary>
    /// <remarks>
    /// Tags help categorize the wiki and improve searchability and filtering based on keywords.
    /// </remarks>
    [MaxLength(256)]
    [DisplayName("Tags")]
    [Comment("Keywords or categories for the project")]
    public string? Tags { get; set; }

    /// <summary>
    /// This field will store the normalized, concatenated values for searching
    /// </summary>
    [Comment("This field will store the normalized, concatenated values for searching")]
    public string? SearchIndex { get; set; }

    /// <summary>
    /// Gets or sets the first characters of the markdown content for the wiki page.
    /// </summary>
    /// <remarks>
    /// The content is stored in markdown format, allowing for rich text editing and display.
    /// </remarks>
    [MaxLength(256)]
    [Comment("Gets or sets the first characters of the markdown content for the wiki page.")]
    public string PreviewText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the markdown content for the wiki page.
    /// </summary>
    /// <remarks>
    /// The content is stored in markdown format, allowing for rich text editing and display.
    /// </remarks>
    [Comment("Gets or sets the markdown content for the wiki page.")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Represents the rank of the search result for the wiki page.
    /// This value is calculated dynamically based on the search query
    /// and is used to order the search results by relevance.
    /// </summary>
    /// <remarks>
    /// The rank is set to a default value of 0 and is updated during specific
    /// queries (e.g., full-text search queries). It is not a persistent value 
    /// for the wiki page, but is dynamically calculated and used in searches.
    /// </remarks>
    [Comment("Represents the rank of the search result during specific queries.")]
    public float Rank { get; set; } = 0;
}