using AutoMapper;
using Huybrechts.App.Data;
using Huybrechts.Core;

namespace Huybrechts.App.Features;

public static class EntityListFlow
{
    internal const int PageSize = 50;

    public class Query
    {
        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchText { get; init; } = string.Empty;

        public string SortOrder { get; init; } = string.Empty;

        public int? Page { get; init; }
    }

    public class Result<TModel>
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
        
        internal IQueryable<TEntity> EntitySet => _dbcontext.Set<TEntity>();

        public Handler(FeatureContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }
    }
}
