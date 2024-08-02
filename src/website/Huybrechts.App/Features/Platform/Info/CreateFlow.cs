using FluentValidation;
using Huybrechts.Core.Platform;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Features.Platform.Info;

public class CreateFlow
{
    public sealed record Command : IRequest<Ulid>
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Name), ResourceType = typeof(PlatformLocalization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(PlatformLocalization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(PlatformLocalization))]
        public string? Remark { get; set; }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(m => m.Name).NotNull().Length(1, 128);
            RuleFor(m => m.Description).Length(1, 256);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Ulid>
    {
        private readonly PlatformContext _dbcontext;

        public Handler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Ulid> Handle(Command message, CancellationToken token)
        {
            var record = new PlatformInfo
            {
                Name = message.Name,
                Description = message.Description,
                Remark = message.Remark
            };

            await _dbcontext.Platforms.AddAsync(record, token);

            await _dbcontext.SaveChangesAsync(token);

            return record.Id;
        }
    }
}
