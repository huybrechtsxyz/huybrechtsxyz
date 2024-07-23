using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using MediatR;
using System.ComponentModel;

namespace Huybrechts.App.Features.Platform;

public class IndexFlow
{
    public sealed record Query : IRequest<Result>
    {
        public string SortOrder { get; init; } = string.Empty;

        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchString { get; init; } = string.Empty;

        public int? Page { get; init; }
    }

    public sealed record Result
    {
        public string CurrentSort { get; init; } = string.Empty;

        public string NameSortParm { get; init; } = string.Empty;

        public string DateSortParm { get; init; } = string.Empty;

        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchString { get; init; } = string.Empty;

        public PaginatedList<Model> Results { get; init; } = [];
    }

    public sealed record Model
    {
        public int Id { get; init; }

        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Remark")]
        public string? Remark { get; set; }
    }

    public sealed class Mapping: Profile
    {
        public Mapping() => CreateProjection<PlatformInfo, Model>();
    }

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {

        }
    }

    public sealed class Handler : IRequestHandler<Query, Result>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public Handler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result> Handle(Query message, CancellationToken token)
        {
            var searchString = message.SearchString ?? message.CurrentFilter;

            IQueryable<PlatformInfo> query = _dbcontext.Platforms;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q => q.Name.Contains(searchString));
            }

            query = message.SortOrder switch
            {
                "name_desc" => query.OrderByDescending(o => o.Name),
                _ => query.OrderBy(o => o.Name)
            };

            int pageSize = 50;
            int pageNumber = (message.SearchString == null ? message.Page : 1) ?? 1;

            var results = await query
                .ProjectTo<Model>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new Result
            {
                CurrentSort = message.SortOrder,
                NameSortParm = string.IsNullOrEmpty(message.SortOrder) ? "name_desc" : "",
                DateSortParm = message.SortOrder == "Date" ? "date_desc" : "Date",
                CurrentFilter = searchString,
                SearchString = searchString,
                Results = results ?? []
            };

            return model;
        }
    }
}
