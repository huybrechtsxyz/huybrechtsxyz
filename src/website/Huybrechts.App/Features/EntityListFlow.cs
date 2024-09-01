using AutoMapper;
using Huybrechts.App.Data;
using Huybrechts.Core;

namespace Huybrechts.App.Features;

public abstract class EntityListFlow
{
    internal const int PageSize = 50;

    public interface IListQuery
    {
        string CurrentFilter { get; init; }

        string SearchText { get; init; }

        string SortOrder { get; init; }

        int? Page { get; init; }
    }

    public record Query : IListQuery
    {
        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchText { get; init; } = string.Empty;

        public string SortOrder { get; init; } = string.Empty;

        public int? Page { get; init; }
    }

    public interface IListResult<TModel>
    {
        public string CurrentFilter { get; init; }

        public string SearchText { get; init; }

        public string SortOrder { get; init; }

        public PaginatedList<TModel> Results { get; init; }
    }

    public record Result<TModel> : IListResult<TModel>
    {
        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchText { get; init; } = string.Empty;

        public string SortOrder { get; init; } = string.Empty;

        public PaginatedList<TModel> Results { get; init; } = [];
    }

    public class Handler<TEntity, TModel> where TEntity : Entity
    {
        protected readonly FeatureContext _dbcontext;
        protected readonly IConfigurationProvider _configuration;
        
        public Handler(FeatureContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }
    }
}
