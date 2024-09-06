using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project.ProjectDesignFlow;

public static class ProjectDesignHelper
{
    public static CreateCommand CreateNew(ProjectInfo Project) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectInfoId = Project.Id,
        ProjectInfoName = Project.Name
    };

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateEntityFound(string name) => Result.Fail(Messages.DUPLICATE_PROJECTDESIGN_NAME.Replace("{0}", name.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid ProjectInfoId, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();
        return await context.Set<ProjectDesign>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                            && pr.ProjectInfoId == ProjectInfoId
                            && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

public record Model
{
    public Ulid Id { get; set; }

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public string ProjectInfoName { get; set; } = string.Empty;

    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    [Display(Name = nameof(Tags), ResourceType = typeof(Localization))]
    public string? Tags { get; set; }

    [Display(Name = nameof(State), ResourceType = typeof(Localization))]
    public string State { get; set; } = string.Empty;

    [Display(Name = nameof(Reason), ResourceType = typeof(Localization))]
    public string? Reason { get; set; }

    [Display(Name = nameof(Environment), ResourceType = typeof(Localization))]
    public string? Environment { get; set; }

    [DataType(DataType.Url)]
    [Display(Name = nameof(Version), ResourceType = typeof(Localization))]
    public string? Version { get; set; }

    [DataType(DataType.Url)]
    [Display(Name = nameof(Dependencies), ResourceType = typeof(Localization))]
    public string? Dependencies { get; set; }

    [Display(Name = nameof(Priority), ResourceType = typeof(Localization))]
    public string? Priority { get; set; }

    [Display(Name = nameof(Risk), ResourceType = typeof(Localization))]
    public string? Risk { get; set; }

    [Display(Name = nameof(Rating), ResourceType = typeof(Localization))]
    public int? Rating { get; set; }

    public string SearchIndex => ModelHelper.GetSearchIndex(Name, Description, Tags);
}

public static class ModelHelper
{
    public static string GetSearchIndex
        (string name, string? description, string? tags)
        => $"{name}~{description}~{tags}".ToLowerInvariant();

    public static void CopyFields(Model model, ProjectDesign entity)
    {
        entity.Name = model.Name.Trim();
        entity.Description = model.Description?.Trim();
        entity.Remark = model.Remark?.Trim();
        entity.Tags = model.Tags?.Trim();
        entity.SearchIndex = model.SearchIndex;

        entity.State = model.State.Trim();
        entity.Reason = model.Reason?.Trim();
        entity.Environment = model.Environment?.Trim();
        entity.Version = model.Version?.Trim();
        entity.Dependencies = model.Dependencies?.Trim();
        entity.Priority = model.Priority?.Trim();
        entity.Risk = model.Risk?.Trim();
        entity.Rating = model.Rating;
    }
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty();
        RuleFor(m => m.ProjectInfoId).NotNull().NotEmpty();
        RuleFor(m => m.Name).NotEmpty().Length(1, 128);
        RuleFor(m => m.Description).Length(0, 256);

        RuleFor(m => m.State).NotEmpty().Length(1, 32);
        RuleFor(m => m.Reason).Length(0, 256);
        RuleFor(m => m.Environment).Length(0, 128);
        RuleFor(m => m.Version).Length(0, 32);
        RuleFor(m => m.Priority).Length(0, 32);
        RuleFor(m => m.Risk).Length(0, 32);
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<ProjectDesign, ListModel>()
        .ForMember(dest => dest.ProjectInfoName, opt => opt.MapFrom(src => src.ProjectInfo.Name));
}

public sealed record ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>>
{
    public Ulid? ProjectInfoId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.ProjectInfoId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult : EntityListFlow.Result<ListModel>
{
    public Ulid? ProjectInfoId { get; set; } = Ulid.Empty;

    public ProjectInfo Project = new();
}

internal sealed class ListHandler :
    EntityListFlow.Handler<ProjectDesign, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        var Project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == message.ProjectInfoId, cancellationToken: token);
        if (Project == null)
            return ProjectDesignHelper.ProjectNotFound(message.ProjectInfoId ?? Ulid.Empty);

        IQueryable<ProjectDesign> query = _dbcontext.Set<ProjectDesign>();

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            string searchFor = searchString.ToLowerInvariant();
            query = query.Where(q =>
                q.ProjectInfoId == message.ProjectInfoId
                && (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)));
        }
        else
        {
            query = query.Where(q => q.ProjectInfoId == message.ProjectInfoId);
        }

        if (!string.IsNullOrEmpty(message.SortOrder))
        {
            query = query.OrderBy(message.SortOrder);
        }
        else query = query.OrderBy(o => o.Name);

        int pageSize = EntityListFlow.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await query
            .Include(i => i.ProjectInfo)
            .ProjectTo<ListModel>(_configuration)
            .PaginatedListAsync(pageNumber, pageSize);

        var model = new ListResult
        {
            ProjectInfoId = message.ProjectInfoId,
            CurrentFilter = searchString,
            SearchText = searchString,
            SortOrder = message.SortOrder,
            Project = Project,
            Results = results ?? []
        };

        return model;
    }
}

//
// CREATE
//

public sealed record CreateQuery : IRequest<Result<CreateCommand>>
{
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.ProjectInfoId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
{
    public List<SetupState> States { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
{
    public CreateCommandValidator(FeatureContext dbContext) : base()
    {
        RuleFor(x => x.ProjectInfoId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectInfo>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECT_ID.Replace("{0}", m.ProjectInfoId.ToString()));

        RuleFor(x => x).MustAsync(async (model, cancellation) =>
        {
            bool exists = await ProjectDesignHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTDESIGN_NAME.Replace("{0}", x.Name.ToString()))
        .WithName(nameof(CreateCommand.Name));
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
        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([message.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectDesignHelper.ProjectNotFound(message.ProjectInfoId);

        var command = ProjectDesignHelper.CreateNew(Project);

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
        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([message.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectDesignHelper.ProjectNotFound(message.ProjectInfoId);

        if (await ProjectDesignHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId))
            return ProjectDesignHelper.DuplicateEntityFound(message.Name);

        var entity = new ProjectDesign
        {
            Id = message.Id,
            ProjectInfo = Project,
            CreatedDT = DateTime.UtcNow
        };
        ModelHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectDesign>().AddAsync(entity, token);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok(entity.Id);
    }
}

//
// UPDATE
//

public sealed record UpdateQuery : IRequest<Result<UpdateCommand>> { public Ulid Id { get; init; } }

public sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
{
    public UpdateQueryValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEqual(Ulid.Empty);
    }
}

public record UpdateCommand : Model, IRequest<Result> 
{
    public ProjectInfo ProjectInfo { get; set; } = new();

    public List<SetupState> States { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.ProjectInfoId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectInfo>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECT_ID.Replace("{0}", m.ProjectInfoId.ToString()));

        RuleFor(x => x).MustAsync(async (model, cancellation) =>
        {
            bool exists = await ProjectDesignHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId, model.Id);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTDESIGN_NAME.Replace("{0}", x.Name.ToString()))
        .WithName(nameof(CreateCommand.Name));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectDesign, UpdateCommand>()
        .ForMember(dest => dest.ProjectInfoName, opt => opt.MapFrom(src => src.ProjectInfo.Name));
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
        var command = await _dbcontext.Set<ProjectDesign>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectDesignHelper.EntityNotFound(message.Id);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectDesignHelper.ProjectNotFound(command.ProjectInfoId);

        command.ProjectInfo = project;
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
        var entity = await _dbcontext.Set<ProjectDesign>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectDesignHelper.EntityNotFound(message.Id);

        if (await ProjectDesignHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId, entity.Id))
            return ProjectDesignHelper.DuplicateEntityFound(message.Name);

        entity.ModifiedDT = DateTime.UtcNow;
        ModelHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectDesign>().Update(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}

//
// DELETE
//

public sealed record DeleteQuery : IRequest<Result<DeleteCommand>> { public Ulid Id { get; init; } }

public class DeleteQueryValidator : AbstractValidator<DeleteQuery>
{
    public DeleteQueryValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record DeleteCommand : Model, IRequest<Result>
{
    public ProjectInfo ProjectInfo { get; set; } = new();
}

public sealed class DeleteCommandValidator : AbstractValidator<DeleteCommand>
{
    public DeleteCommandValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
    }
}

internal sealed class DeleteCommandMapping : Profile
{
    public DeleteCommandMapping() => 
        CreateProjection<ProjectDesign, DeleteCommand>()
        .ForMember(dest => dest.ProjectInfoName, opt => opt.MapFrom(src => src.ProjectInfo.Name));
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
        var command = await _dbcontext.Set<ProjectDesign>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectDesignHelper.EntityNotFound(message.Id);

        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectDesignHelper.ProjectNotFound(command.ProjectInfoId);

        command.ProjectInfo = Project;

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
        var entity = await _dbcontext.Set<ProjectDesign>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectDesignHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectDesign>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}