using AutoMapper;
using AutoMapper.QueryableExtensions;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.App.Features.Platform.Info;

public class DeleteFlow
{
    public record Query : IRequest<Model>
    {
        public Ulid Id { get; init; }
    }

    public record Model
    {
        public Ulid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? Remark { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<PlatformInfo, Model>();
        }
    }

    public class QueryHandler : IRequestHandler<Query, Model>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Model> Handle(Query message, CancellationToken token)
        {
            return await _dbcontext.Platforms
                .Where(s => s.Id == message.Id)
                .ProjectTo<Model>(_configuration)
                .SingleOrDefaultAsync(token) ??
                new Model();
        }
    }
}
