using AutoMapper;
using Huybrechts.App.Data;
using Huybrechts.Core;

namespace Huybrechts.App.Features.EntityFlow;

/// <summary>
/// Interface representing a query for listing entities with optional filters, sorting, and pagination.
/// </summary>
public interface IListQuery
{
    /// <summary>
    /// Gets the current filter applied to the query, used to narrow down the results.
    /// </summary>
    string CurrentFilter { get; init; }

    /// <summary>
    /// Gets the search text used for filtering entities.
    /// </summary>
    string SearchText { get; init; }

    /// <summary>
    /// Gets the sort order used to arrange the results (e.g., ascending or descending).
    /// </summary>
    string SortOrder { get; init; }

    /// <summary>
    /// Gets the page number for pagination. If null, defaults to the first page.
    /// </summary>
    int? Page { get; init; }
}


/// <summary>
/// Record representing a query with filters, search text, sorting, and pagination for listing entities.
/// </summary>
public record ListQuery : IListQuery
{
    /// <summary>
    /// Constant representing the size of each page in the paginated list.
    /// </summary>
    internal const int PageSize = 50;

    /// <inheritdoc />
    public string CurrentFilter { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SearchText { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SortOrder { get; init; } = string.Empty;

    /// <inheritdoc />
    public int? Page { get; init; }
}


/// <summary>
/// Interface representing the result of a query for listing entities, including pagination, filters, and sorting.
/// </summary>
/// <typeparam name="TModel">The type of the model being returned in the result set.</typeparam>
public interface IListResult<TModel>
{
    /// <summary>
    /// Gets the current filter that was applied during the query.
    /// </summary>
    string CurrentFilter { get; init; }

    /// <summary>
    /// Gets the search text used to filter the results.
    /// </summary>
    string SearchText { get; init; }

    /// <summary>
    /// Gets the sort order used to arrange the result list.
    /// </summary>
    string SortOrder { get; init; }

    /// <summary>
    /// Gets the paginated list of results of type <typeparamref name="TModel"/>.
    /// </summary>
    PaginatedList<TModel> Results { get; init; }
}


/// <summary>
/// Record representing the result of a query for listing entities, with pagination, filters, and sorting.
/// </summary>
/// <typeparam name="TModel">The type of the model being returned in the result set.</typeparam>
public record ListResult<TModel> : IListResult<TModel>
{
    /// <inheritdoc />
    public string CurrentFilter { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SearchText { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SortOrder { get; init; } = string.Empty;

    /// <inheritdoc />
    public PaginatedList<TModel> Results { get; init; } = [];
}


/// <summary>
/// Base class for handling entity list queries, including database context access and AutoMapper configuration.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being queried.</typeparam>
/// <typeparam name="TModel">The type of the model to which the entity will be projected for presentation.</typeparam>
public class ListHandler<TEntity, TModel> where TEntity : Entity
{
    /// <summary>
    /// The database context used to query entities.
    /// </summary>
    protected readonly FeatureContext _dbcontext;

    /// <summary>
    /// The AutoMapper configuration used to map entities to models.
    /// </summary>
    protected readonly IConfigurationProvider _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Handler{TEntity, TModel}"/> class.
    /// </summary>
    /// <param name="dbcontext">The database context used to query entities.</param>
    /// <param name="configuration">The AutoMapper configuration used to map entities to models.</param>
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
    {
        _dbcontext = dbcontext;
        _configuration = configuration;
    }
}
