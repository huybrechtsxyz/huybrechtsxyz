using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Features.Platform.Info;

public class EditFlow
{
    public record Query : IRequest<Command>
    {
        public Ulid? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Command : IRequest
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Name), ResourceType = typeof(PlatformLocalization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(PlatformLocalization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(PlatformLocalization))]
        public string? Remark { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(m => m.Name).NotNull().Length(1, 128);
            RuleFor(m => m.Description).Length(1, 256);
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<PlatformInfo, Command>();
    }

    public class QueryHandler : IRequestHandler<Query, Command>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Command> Handle(Query message, CancellationToken token)
        {
            return await _dbcontext.Platforms
                .Where(s => s.Id == message.Id)
                .ProjectTo<Command>(_configuration)
                .SingleOrDefaultAsync(token) ??
                new Command();
        }
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        private readonly PlatformContext _dbcontext;

        public CommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task Handle(Command message, CancellationToken token)
        {
            var record = await _dbcontext.Platforms.FindAsync(message.Id, token) ??
                throw new InvalidOperationException($"Unable to find platform with ID {message.Id}");

            record.Name = message.Name;
            record.Description = message.Description;
            record.Remark = message.Remark;

            _dbcontext.Platforms.Update(record);
            await _dbcontext.SaveChangesAsync(token);
        }
    }
}
