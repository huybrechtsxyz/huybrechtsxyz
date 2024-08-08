using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Internal;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Platform;

public static class PlatformInfoFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Provider), ResourceType = typeof(Localization))]
        public PlatformProvider Provider { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Name).NotNull().Length(1, 128);
            RuleFor(m => m.Description).Length(1, 256);
        }
    }

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    //
    // LIST
    //

    public sealed record ListModel : Model
    {
    }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() => CreateProjection<PlatformInfo, ListModel>();
    }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<ListResult>
    {
    }

    internal sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
    }

    internal sealed class ListHandler :
        EntityListFlow.Handler<PlatformInfo, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<ListResult> Handle(ListQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlatformInfo> query = _dbcontext.Set<PlatformInfo>();

            var searchString = request.SearchText ?? request.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q => 
                    q.Name.Contains(searchString)
                    || (q.Description.HasValue() && q.Description!.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(request.SortOrder))
            {
                query = query.OrderBy(request.SortOrder);
            }
            else query = query.OrderBy(o => o.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;
            var results = await query
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new ListResult
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Results = results ?? []
            };

            return model;
        }
    }

    //
    // CREATE
    //

    public static CreateCommand CreateNew() => new()
    {
        Id = Ulid.NewUlid()
    };

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
    {
    }

    internal sealed class CreateValidator : ModelValidator<CreateCommand>
    {
        public CreateValidator() : base()
        {
        }
    }

    internal sealed class CreateHandler : IRequestHandler<CreateCommand, Result<Ulid>>
    {
        private readonly PlatformContext _dbcontext;

        public CreateHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<Ulid>> Handle(CreateCommand request, CancellationToken token)
        {
            var record = new PlatformInfo
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Provider = request.Provider,
                Remark = request.Remark,
                CreatedDT = DateTime.UtcNow
            };
            await _dbcontext.Set<PlatformInfo>().AddAsync(record, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(record.Id);
        }
    }

    //
    // UPDATE
    //

    public sealed record UpdateQuery : IRequest<UpdateCommand?>
    {
        public Ulid? Id { get; init; }
    }

    internal sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
    {
        public UpdateQueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record UpdateCommand : Model, IRequest<Result>
    {
    }

    internal class UpdateCommandValidator : ModelValidator<UpdateCommand>
    {
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => CreateProjection<PlatformInfo, UpdateCommand>();
    }

    internal class UpdateQueryHandler : IRequestHandler<UpdateQuery, UpdateCommand?>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public UpdateQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<UpdateCommand?> Handle(UpdateQuery request, CancellationToken token)
        {
            return await _dbcontext.Set<PlatformInfo>()
                .Where(s => s.Id == request.Id)
                .ProjectTo<UpdateCommand>(_configuration)
                .SingleOrDefaultAsync(token);
        }
    }

    internal class UpdateCommandHandler : IRequestHandler<UpdateCommand, Result>
    {
        private readonly PlatformContext _dbcontext;

        public UpdateCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(UpdateCommand command, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformInfo>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            record.Name = command.Name;
            record.Description = command.Description;
            record.Provider = command.Provider;
            record.Remark = command.Remark;
            record.ModifiedDT = DateTime.UtcNow;

            _dbcontext.Set<PlatformInfo>().Update(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // DELETE
    //

    public sealed record DeleteQuery : IRequest<DeleteCommand>
    {
        public Ulid? Id { get; init; }
    }

    internal class DeleteQueryValidator : AbstractValidator<DeleteQuery>
    {
        public DeleteQueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public sealed record DeleteCommand : Model, IRequest<Result>
    {
    }

    internal sealed class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    internal sealed class DeleteCommandMapping : Profile
    {
        public DeleteCommandMapping()
        {
            CreateProjection<PlatformInfo, DeleteCommand>();
        }
    }

    internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, DeleteCommand?>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public DeleteQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<DeleteCommand?> Handle(DeleteQuery request, CancellationToken token)
        {
            return await _dbcontext.Set<PlatformInfo>()
                .Where(s => s.Id == request.Id)
                .ProjectTo<DeleteCommand>(_configuration)
                .SingleOrDefaultAsync(token);
        }
    }

    internal class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result>
    {
        private readonly PlatformContext _dbcontext;

        public DeleteCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(DeleteCommand command, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformInfo>().FindAsync([command.Id], cancellationToken: token);
            if(record is null)
                return RecordNotFound(command.Id);
            _dbcontext.Set<PlatformInfo>().Remove(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }
}
