﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project;

public static class ProjectInfoFlow
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

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }

        public string SearchIndex => ModelHelper.GetSearchIndex(Code, Name, Description, Tags);

        [Display(Name = nameof(Tags), ResourceType = typeof(Localization))]
        public string? Tags { get; set; }

        [Display(Name = nameof(State), ResourceType = typeof(Localization))]
        public string State { get; set; } = string.Empty;

        [Display(Name = nameof(Reason), ResourceType = typeof(Localization))]
        public string? Reason { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = nameof(StartDate), ResourceType = typeof(Localization))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = nameof(TargetDate), ResourceType = typeof(Localization))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? TargetDate { get; set; }

        [Display(Name = nameof(Priority), ResourceType = typeof(Localization))]
        public string? Priority { get; set; }

        [Display(Name = nameof(Risk), ResourceType = typeof(Localization))]
        public string? Risk { get; set; }

        [Display(Name = nameof(Effort), ResourceType = typeof(Localization))]
        public int? Effort { get; set; }

        [Display(Name = nameof(BusinessValue), ResourceType = typeof(Localization))]
        public int? BusinessValue { get; set; }

        [Display(Name = nameof(Rating), ResourceType = typeof(Localization))]
        public int? Rating { get; set; }
    }

    public static class ModelHelper
    {
        public static string GetSearchIndex
            (string code, string name, string? description, string? tags)
            => $"{code}~{name}~{description}~{tags}".ToLowerInvariant();

        public static void CopyFields(Model model, ProjectInfo entity)
        {
            entity.Name = model.Name.Trim();
            entity.Description = model.Description?.Trim();
            entity.Remark = model.Remark?.Trim();
            entity.Tags = model.Tags?.Trim();
            entity.SearchIndex = model.SearchIndex;

            entity.State = model.State.Trim();
            entity.Reason = model.Reason?.Trim();
            entity.StartDate = model.StartDate;
            entity.TargetDate = model.TargetDate;
            entity.Priority = model.Priority?.Trim();
            entity.Risk = model.Risk?.Trim();
            entity.Effort = model.Effort;
            entity.BusinessValue = model.BusinessValue;
            entity.Rating = model.Rating;
        }
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.Code).NotEmpty().Length(1, 32);
            RuleFor(m => m.Name).NotEmpty().Length(1, 128);
            RuleFor(m => m.Description).Length(0, 256);

            RuleFor(m => m.State).NotEmpty().Length(1, 32);
            RuleFor(m => m.Reason).Length(0, 256);
            RuleFor(m => m.Priority).Length(0, 32);
            RuleFor(m => m.Risk).Length(0, 32);
        }
    }

    private static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateCodeFound(string code) => Result.Fail(Messages.DUPLICATE_PROJECT_CODE.Replace("{0}", code.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateCodeAsync(DbContext context, string code, Ulid? currentId = null)
    {
        code = code.ToUpper().Trim();

        return await context.Set<ProjectInfo>()
            .AnyAsync(pr => pr.Code.ToUpper() == code
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

    //
    // LIST
    //

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<ProjectInfo, ListModel>(); }

    public sealed record ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>> { }

    public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed record ListResult : EntityListFlow.Result<ListModel> { }

    internal sealed class ListHandler :
        EntityListFlow.Handler<ProjectInfo, ListModel>,
        IRequestHandler<ListQuery, Result<ListResult>>
    {
        public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
        {
            IQueryable<ProjectInfo> query = _dbcontext.Set<ProjectInfo>();

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

    public sealed record CreateQuery : IRequest<Result<CreateCommand>> { }

    public sealed class CreateQueryValidator : AbstractValidator<CreateQuery> { public CreateQueryValidator() { } }

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
    {
        public List<SetupState> States { get; set; } = [];
    }

    public sealed class CreateValidator : ModelValidator<CreateCommand> 
    {
        public CreateValidator(FeatureContext dbContext)
        {
            RuleFor(x => x.Code).MustAsync(async (code, cancellation) =>
            {
                bool exists = await IsDuplicateCodeAsync(dbContext, code);
                return !exists;
            })
            .WithMessage(x => Messages.DUPLICATE_PROJECT_CODE.Replace("{0}", x.Code.ToString()))
            .WithState(x => x.Code);
        }
    }

    internal class CreateQueryHandler : IRequestHandler<CreateQuery, Result<CreateCommand>>
    {
        private readonly FeatureContext _dbcontext;

        public CreateQueryHandler(FeatureContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<CreateCommand>> Handle(CreateQuery message, CancellationToken token)
        {
            CreateCommand command = CreateNew();

            command.States = await Setup.SetupStateFlow.SetupStateHelper.GetProjectStatesAync(_dbcontext);

            return Result.Ok(command);
        }
    }

    internal sealed class CreateCommandHandler : IRequestHandler<CreateCommand, Result<Ulid>>
    {
        private readonly FeatureContext _dbcontext;

        public CreateCommandHandler(FeatureContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<Ulid>> Handle(CreateCommand message, CancellationToken token)
        {
            if (await IsDuplicateCodeAsync(_dbcontext, message.Code))
                return DuplicateCodeFound(message.Code);

            var entity = new ProjectInfo
            {
                Id = message.Id,
                ParentId = Ulid.Empty,
                CreatedDT = DateTime.UtcNow,
            };
            ModelHelper.CopyFields(message, entity);

            await _dbcontext.Set<ProjectInfo>().AddAsync(entity, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(entity.Id);
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

    public record UpdateCommand : Model, IRequest<Result> 
    {
        public List<SetupState> States { get; set; } = [];
    }

    public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
    {
        public UpdateCommandValidator(FeatureContext dbContext)
        {
            RuleFor(x => x).MustAsync(async (rec, cancellation) =>
            {
                bool exists = await IsDuplicateCodeAsync(dbContext, rec.Name, rec.Id);
                return !exists;
            })
            .WithMessage(x => Messages.DUPLICATE_PROJECT_CODE.Replace("{0}", x.Name.ToString()))
            .WithState(x => x.Code);
        }
    }

    internal class UpdateCommandMapping : Profile 
    { 
        public UpdateCommandMapping() => CreateProjection<ProjectInfo, UpdateCommand>(); 
    }

    internal class UpdateQueryHandler : IRequestHandler<UpdateQuery, Result<UpdateCommand>>
    {
        private readonly FeatureContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public UpdateQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<UpdateCommand>> Handle(UpdateQuery message, CancellationToken token)
        {
            var command = await _dbcontext.Set<ProjectInfo>()
                .ProjectTo<UpdateCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return EntityNotFound(message.Id ?? Ulid.Empty);

            command.States = await Setup.SetupStateFlow.SetupStateHelper.GetProjectStatesAync(_dbcontext);

            return Result.Ok(command);
        }
    }

    internal class UpdateCommandHandler : IRequestHandler<UpdateCommand, Result>
    {
        private readonly FeatureContext _dbcontext;

        public UpdateCommandHandler(FeatureContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(UpdateCommand message, CancellationToken token)
        {
            var entity = await _dbcontext.Set<ProjectInfo>().FindAsync([message.Id], cancellationToken: token);
            if (entity is null)
                return EntityNotFound(message.Id);

            entity.ModifiedDT = DateTime.UtcNow;
            ModelHelper.CopyFields(message, entity);

            _dbcontext.Set<ProjectInfo>().Update(entity);
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
            CreateProjection<ProjectInfo, DeleteCommand>();
        }
    }

    internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, Result<DeleteCommand>>
    {
        private readonly FeatureContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public DeleteQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<DeleteCommand>> Handle(DeleteQuery message, CancellationToken token)
        {
            var command = await _dbcontext.Set<ProjectInfo>()
                .ProjectTo<DeleteCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return EntityNotFound(message.Id ?? Ulid.Empty);

            return Result.Ok(command);
        }
    }

    internal class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result>
    {
        private readonly FeatureContext _dbcontext;

        public DeleteCommandHandler(FeatureContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(DeleteCommand message, CancellationToken token)
        {
            var entity = await _dbcontext.Set<ProjectInfo>().FindAsync([message.Id], cancellationToken: token);
            if(entity is null)
                return EntityNotFound(message.Id);

            _dbcontext.Set<ProjectInfo>().Remove(entity);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }
}