using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Features.Platform.Info;

public class IndexFlow
{
    public sealed record Query : IRequest<Result>
    {
        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchText { get; init; } = string.Empty;

        public int? Page { get; init; }
    }

    public sealed record Result
    {
        public string CurrentFilter { get; init; } = string.Empty;

        public string SearchText { get; init; } = string.Empty;

        public PaginatedList<Model> Results { get; init; } = [];
    }

    public sealed record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Name), ResourceType = typeof(PlatformLocalization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(PlatformLocalization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(PlatformLocalization))]
        public string? Remark { get; set; }
    }

    public sealed class Mapping : Profile
    {
        public Mapping() => CreateProjection<PlatformInfo, Model>();
    }

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {

        }
    }

    internal sealed class Handler : IRequestHandler<Query, Result>
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
            IQueryable<PlatformInfo> query = _dbcontext.Platforms;

            var searchString = message.SearchText ?? message.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q => q.Name.Contains(searchString) || q.Description.Contains(searchString));
            }
            
            query.OrderBy(o => o.Name);

            int pageSize = 50;
            int pageNumber = message.Page ?? 1;
            var results = await query
                .ProjectTo<Model>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new Result
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                Results = results ?? []
            };

            return model;
        }
    }
}
