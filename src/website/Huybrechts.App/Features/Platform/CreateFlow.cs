using FluentValidation;
using Huybrechts.Core.Platform;
using MediatR;
using System.ComponentModel;

namespace Huybrechts.App.Features.Platform;

public class CreateFlow
{
    public sealed record Command : IRequest<int>
    {
        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Remark")]
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

    public sealed class Handler : IRequestHandler<Command, int>
    {
        private readonly PlatformContext _dbcontext;

        public Handler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<int> Handle(Command message, CancellationToken token)
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
