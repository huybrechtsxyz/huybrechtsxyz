using AutoMapper;
using AutoMapper.QueryableExtensions;
using Huybrechts.App.Data;
using Huybrechts.Core;
using MediatR;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features;

public static class EntityListFlow
{
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
        private readonly FeatureContext _dbcontext;
        private readonly IConfigurationProvider _configuration;
        
        internal IQueryable<TEntity> EntitySet => _dbcontext.Set<TEntity>();

        internal string? SetWhere { get; set; }

        internal string? OrderBy { get; set; }

        public Handler(FeatureContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<TModel>> Handle(Query request, CancellationToken token)
        {
            IQueryable<TEntity> query = EntitySet;

            var searchString = request.SearchText ?? request.CurrentFilter;
            if (!string.IsNullOrEmpty(SetWhere) && !string.IsNullOrEmpty(searchString))
            {
                query = query.Where(SetWhere, searchString);
            }

            if (OrderBy is not null)
            {
                query.OrderBy(OrderBy);
            }
            
            int pageSize = 50;
            int pageNumber = request.Page ?? 1;
            var results = await query
                .ProjectTo<TModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new Result<TModel>
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Results = results ?? []
            };

            return model;
        }
    }
}
