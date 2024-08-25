﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Huybrechts.App.Features.Setup;

public static class SetupUnitFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Code), ResourceType = typeof(Localization))]
        public string Code { get; set; } = string.Empty;

        [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(UnitType), ResourceType = typeof(Localization))]
        public SetupUnitType UnitType { get; set; }

        [Display(Name = nameof(Precision), ResourceType = typeof(Localization))]
        public int Precision { get; set; } = 2;

        [Display(Name = nameof(PrecisionType), ResourceType = typeof(Localization))]
        public MidpointRounding PrecisionType { get; set; } = MidpointRounding.ToEven;

        [Precision(18, 10)]
        [Display(Name = nameof(Factor), ResourceType = typeof(Localization))]
        public decimal Factor { get; set; } = 1.0m;

        [Display(Name = nameof(IsBase), ResourceType = typeof(Localization))]
        public bool IsBase { get; set; } = false;

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }

        public string SearchIndex => $"{Name}~{Description}".ToLowerInvariant();
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.Code).NotEmpty().Length(1, 10);
            RuleFor(m => m.Name).NotEmpty().Length(1, 128);
            RuleFor(m => m.Description).Length(0, 256);
        }
    }

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPUNIT_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateFound(string name) => Result.Fail(Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", name.ToString()));

    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();

        return await context.Set<SetupUnit>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

    //
    // LIST
    //

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<SetupUnit, ListModel>(); }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>> { }

    public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel> { }

    internal sealed class ListHandler :
        EntityListFlow.Handler<SetupUnit, ListModel>,
        IRequestHandler<ListQuery, Result<ListResult>>
    {
        public ListHandler(SetupContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
        {
            IQueryable<SetupUnit> query = _dbcontext.Set<SetupUnit>();

            var searchString = message.SearchText ?? message.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchFor = searchString.ToLower();
                query = query.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor));
            }

            if (!string.IsNullOrEmpty(message.SortOrder))
            {
                query = query.OrderBy(message.SortOrder);
            }
            else query = query.OrderBy(o => o.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;
            var results = await query
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new ListResult
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = message.SortOrder,
                Results = results ?? []
            };

            return model;
        }
    }

    //
    // CREATE
    //

    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

    public sealed class CreateValidator : ModelValidator<CreateCommand> 
    {
        public CreateValidator(SetupContext dbContext) :
            base()
        {
            RuleFor(x => x.Name).MustAsync(async (name, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, name);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", x.Name.ToString()));
        }
    }

    internal sealed class CreateHandler : IRequestHandler<CreateCommand, Result<Ulid>>
    {
        private readonly SetupContext _dbcontext;

        public CreateHandler(SetupContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<Ulid>> Handle(CreateCommand message, CancellationToken token)
        {
            if (await IsDuplicateNameAsync(_dbcontext, message.Name))
                return DuplicateFound(message.Name);

            var record = new SetupUnit
            {
                Id = message.Id,
                Code = message.Code.Trim(),
                Name = message.Name.Trim(),
                Description = message.Description?.Trim(),
                UnitType = message.UnitType,
                Factor = message.Factor,
                IsBase = message.IsBase,
                Precision = message.Precision,
                PrecisionType = message.PrecisionType,
                Remark = message.Remark?.Trim(),
                SearchIndex = message.SearchIndex,
                CreatedDT = DateTime.UtcNow
            };
            await _dbcontext.Set<SetupUnit>().AddAsync(record, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(record.Id);
        }
    }

    //
    // DEFAULTS
    //

    public sealed record CreateDefaultsCommand : IRequest<Result>
    {
    }

    public sealed class CreateDefaultsHandler
    {
        private readonly SetupContext _dbcontext;

        public CreateDefaultsHandler(SetupContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(CreateDefaultsCommand message, CancellationToken token)
        {
            var filePath = Path.Combine("./systemunits.json");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The systemunits.json file was not found.", filePath);
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<SetupUnit>? units = await JsonSerializer.DeserializeAsync<List<SetupUnit>>(stream, cancellationToken: token);
            
            foreach(var item in units ?? [])
            {
                if (await IsDuplicateNameAsync(_dbcontext, item.Name))
                    continue;

                var record = new SetupUnit
                {
                    Id = item.Id,
                    Code = item.Code.Trim(),
                    Name = item.Name.Trim(),
                    Description = item.Description?.Trim(),
                    UnitType = item.UnitType,
                    Factor = item.Factor,
                    IsBase = item.IsBase,
                    Precision = item.Precision,
                    PrecisionType = item.PrecisionType,
                    Remark = item.Remark?.Trim(),
                    SearchIndex = item.SearchIndex,
                    CreatedDT = DateTime.UtcNow
                };

                await _dbcontext.Set<SetupUnit>().AddAsync(record, token);
            }
            
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // UPDATE
    //

    public sealed record UpdateQuery : IRequest<Result<UpdateCommand>> { public Ulid? Id { get; init; } }

    public sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
    {
        public UpdateQueryValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEqual(Ulid.Empty);
        }
    }

    public record UpdateCommand : Model, IRequest<Result> { }

    public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
    {
        public UpdateCommandValidator(SetupContext dbContext)
        {
            RuleFor(x => x).MustAsync(async (rec, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, rec.Name, rec.Id);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", x.Name.ToString()));
        }
    }

    internal class UpdateCommandMapping : Profile 
    { 
        public UpdateCommandMapping() => CreateProjection<SetupUnit, UpdateCommand>(); 
    }

    internal class UpdateQueryHandler : IRequestHandler<UpdateQuery, Result<UpdateCommand>>
    {
        private readonly SetupContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public UpdateQueryHandler(SetupContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<UpdateCommand>> Handle(UpdateQuery message, CancellationToken token)
        {
            var command = await _dbcontext.Set<SetupUnit>()
                .ProjectTo<UpdateCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return RecordNotFound(message.Id ?? Ulid.Empty);

            return Result.Ok(command);
        }
    }

    internal class UpdateCommandHandler : IRequestHandler<UpdateCommand, Result>
    {
        private readonly SetupContext _dbcontext;

        public UpdateCommandHandler(SetupContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(UpdateCommand message, CancellationToken token)
        {
            var record = await _dbcontext.Set<SetupUnit>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, record.Id))
                return DuplicateFound(message.Name);

            record.Name = message.Name.Trim();
            record.Description = message.Description?.Trim();
            record.UnitType = message.UnitType;
            record.Factor = message.Factor;
            record.IsBase = message.IsBase;
            record.Precision = message.Precision;
            record.PrecisionType = message.PrecisionType;
            record.Remark = message.Remark?.Trim();
            record.SearchIndex = message.SearchIndex;
            record.ModifiedDT = DateTime.UtcNow;

            _dbcontext.Set<SetupUnit>().Update(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // DELETE
    //

    public sealed record DeleteQuery : IRequest<Result<DeleteCommand>> { public Ulid? Id { get; init; } }

    public class DeleteQueryValidator : AbstractValidator<DeleteQuery>
    {
        public DeleteQueryValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public sealed record DeleteCommand : Model, IRequest<Result> { }

    public sealed class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    internal sealed class DeleteCommandMapping : Profile
    {
        public DeleteCommandMapping()
        {
            CreateProjection<SetupUnit, DeleteCommand>();
        }
    }

    internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, Result<DeleteCommand>>
    {
        private readonly SetupContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public DeleteQueryHandler(SetupContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<DeleteCommand>> Handle(DeleteQuery message, CancellationToken token)
        {
            var command = await _dbcontext.Set<SetupUnit>()
                .ProjectTo<DeleteCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return RecordNotFound(message.Id ?? Ulid.Empty);

            return Result.Ok(command);
        }
    }

    internal class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result>
    {
        private readonly SetupContext _dbcontext;

        public DeleteCommandHandler(SetupContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(DeleteCommand message, CancellationToken token)
        {
            var record = await _dbcontext.Set<SetupUnit>().FindAsync([message.Id], cancellationToken: token);
            if(record is null)
                return RecordNotFound(message.Id);

            _dbcontext.Set<SetupUnit>().Remove(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }
}
