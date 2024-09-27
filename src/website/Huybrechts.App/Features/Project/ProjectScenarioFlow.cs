using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Project;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project.ProjectScenarioFlow;

public static class ProjectScenarioHelper
{
    public static string GetSearchIndex
        (string name, string? description, string? tags)
        => $"{name}~{description}~{tags}".ToLowerInvariant();

    public static void CopyFields(Model model, ProjectScenario entity)
    {
        entity.Name = model.Name.Trim();
        entity.Description = model.Description?.Trim();
        entity.Remark = model.Remark?.Trim();
        entity.Tags = model.Tags?.Trim();
        entity.SearchIndex = model.SearchIndex;
    }

    public static CreateCommand CreateNew(ProjectInfo Project) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectInfoId = Project.Id,
        ProjectInfoName = Project.Name
    };

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTSCENARIO_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateEntityFound(string name) => Result.Fail(Messages.DUPLICATE_PROJECTSCENARIO_NAME.Replace("{0}", name.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid ProjectInfoId, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();
        return await context.Set<ProjectScenario>()
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

    public string SearchIndex => ProjectScenarioHelper.GetSearchIndex(Name, Description, Tags);
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty();
        RuleFor(m => m.ProjectInfoId).NotNull().NotEmpty();
        RuleFor(m => m.Name).NotEmpty().Length(1, 128);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<ProjectScenario, ListModel>()
        .ForMember(dest => dest.ProjectInfoName, opt => opt.MapFrom(src => src.ProjectInfo.Name));
}

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>>
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

public sealed record ListResult : EntityFlow.ListResult<ListModel>
{
    public Ulid? ProjectInfoId { get; set; } = Ulid.Empty;

    public ProjectInfo Project = new();
}

internal sealed class ListHandler :
    EntityFlow.ListHandler<ProjectScenario, ListModel>,
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
            return ProjectScenarioHelper.ProjectNotFound(message.ProjectInfoId ?? Ulid.Empty);

        IQueryable<ProjectScenario> query = _dbcontext.Set<ProjectScenario>();

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

        int pageSize = EntityFlow.ListQuery.PageSize;
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

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

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
            bool exists = await ProjectScenarioHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTSCENARIO_NAME.Replace("{0}", x.Name.ToString()))
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
            return ProjectScenarioHelper.ProjectNotFound(message.ProjectInfoId);

        var command = ProjectScenarioHelper.CreateNew(Project);

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
            return ProjectScenarioHelper.ProjectNotFound(message.ProjectInfoId);

        if (await ProjectScenarioHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId))
            return ProjectScenarioHelper.DuplicateEntityFound(message.Name);

        var entity = new ProjectScenario
        {
            Id = message.Id,
            ProjectInfo = Project,
            CreatedDT = DateTime.UtcNow
        };
        ProjectScenarioHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectScenario>().AddAsync(entity, token);
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
            bool exists = await ProjectScenarioHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId, model.Id);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTSCENARIO_NAME.Replace("{0}", x.Name.ToString()))
        .WithName(nameof(CreateCommand.Name));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectScenario, UpdateCommand>()
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
        var command = await _dbcontext.Set<ProjectScenario>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectScenarioHelper.ProjectNotFound(command.ProjectInfoId);

        command.ProjectInfo = project;

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
        var entity = await _dbcontext.Set<ProjectScenario>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        if (await ProjectScenarioHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId, entity.Id))
            return ProjectScenarioHelper.DuplicateEntityFound(message.Name);

        entity.ModifiedDT = DateTime.UtcNow;
        ProjectScenarioHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectScenario>().Update(entity);
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
        CreateProjection<ProjectScenario, DeleteCommand>()
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
        var command = await _dbcontext.Set<ProjectScenario>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectScenarioHelper.ProjectNotFound(command.ProjectInfoId);

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
        var entity = await _dbcontext.Set<ProjectScenario>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectScenario>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}

//
// COPY
//

public sealed record CopyQuery : IRequest<Result<CopyCommand>> { public Ulid Id { get; init; } }

public class CopyQueryValidator : AbstractValidator<CopyQuery>
{
    public CopyQueryValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CopyCommand : Model, IRequest<Result>
{
    public ProjectInfo ProjectInfo { get; set; } = new();
}

public sealed class CopyCommandValidator : AbstractValidator<CopyCommand>
{
    public CopyCommandValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
    }
}

internal sealed class CopyCommandMapping : Profile
{
    public CopyCommandMapping() =>
        CreateProjection<ProjectScenario, CopyCommand>()
        .ForMember(dest => dest.ProjectInfoName, opt => opt.MapFrom(src => src.ProjectInfo.Name));
}

internal sealed class CopyEntityMapping : Profile
{
    public CopyEntityMapping()
    {
        // Map Scenario to Scenario (can be another entity as well)
        CreateMap<ProjectScenario, ProjectScenario>()
            .ForMember(dst => dst.ProjectInfo, opt => opt.Ignore())
            .ForMember(dst => dst.Units, opt => opt.Ignore())
            .ForMember(dst => dst.CreatedDT, opt => opt.Ignore())
            .ForMember(dst => dst.ModifiedDT, opt => opt.Ignore())
            .ForMember(dst => dst.TimeStamp, opt => opt.Ignore());
    }
}

internal sealed class CopyEntityUnitMapping : Profile
{
    public CopyEntityUnitMapping()
    {
        // Map Scenario to Scenario (can be another entity as well)
        CreateMap<ProjectScenarioUnit, ProjectScenarioUnit>()
            .ForMember(dst => dst.ProjectScenario, opt => opt.Ignore())
            .ForMember(dst => dst.ProjectScenarioId, opt => opt.Ignore())
            .ForMember(dst => dst.SetupUnit, opt => opt.Ignore())
            .ForMember(dst => dst.SetupUnitId, opt => opt.Ignore())
            .ForMember(dst => dst.CreatedDT, opt => opt.Ignore())
            .ForMember(dst => dst.ModifiedDT, opt => opt.Ignore())
            .ForMember(dst => dst.TimeStamp, opt => opt.Ignore());
    }
}

internal sealed class CopyQueryHandler : IRequestHandler<CopyQuery, Result<CopyCommand>>
{
    private readonly FeatureContext _dbcontext;
    private readonly IConfigurationProvider _configuration;

    public CopyQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
    {
        _dbcontext = dbcontext;
        _configuration = configuration;
    }

    public async Task<Result<CopyCommand>> Handle(CopyQuery message, CancellationToken token)
    {
        var command = await _dbcontext.Set<ProjectScenario>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<CopyCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectScenarioHelper.ProjectNotFound(command.ProjectInfoId);

        command.ProjectInfo = Project;

        return Result.Ok(command);
    }
}

internal class CopyCommandHandler : IRequestHandler<CopyCommand, Result>
{
    private readonly FeatureContext _dbcontext;
    private readonly IMapper _mapper;

    public CopyCommandHandler(FeatureContext dbcontext, IMapper mapper)
    {
        _dbcontext = dbcontext;
        _mapper = mapper;
    }

    public async Task<Result> Handle(CopyCommand message, CancellationToken token)
    {
        await _dbcontext.BeginTransactionAsync(token);

        var entity = await _dbcontext.Set<ProjectScenario>()
            .Include(i => i.ProjectInfo)
            .Include(i => i.Units)
            .FirstOrDefaultAsync(f => f.Id == message.Id, token);
        if (entity is null)
            return ProjectScenarioHelper.EntityNotFound(message.Id);

        var setupUnits = await SetupUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

        ProjectScenario newEntity = _mapper.Map<ProjectScenario>(entity);
        newEntity.Id = Ulid.NewUlid();
        newEntity.ProjectInfo = entity.ProjectInfo;
        newEntity.CreatedDT = DateTime.UtcNow;
        newEntity.Name = (newEntity.Name.Length > 128 - " (Copy)".Length ? newEntity.Name[..(128 - 7)] : newEntity.Name) + " (Copy)";
        await _dbcontext.Set<ProjectScenario>().AddAsync(newEntity, token);

        foreach(var unit in entity.Units)
        {
            ProjectScenarioUnit newUnit = _mapper.Map<ProjectScenarioUnit>(unit);
            newUnit.Id = Ulid.NewUlid();
            newUnit.ProjectScenario = newEntity;
            newUnit.CreatedDT = DateTime.UtcNow;

            newUnit.SetupUnit = setupUnits.FirstOrDefault(f => f.Id == unit.SetupUnitId);

            await _dbcontext.Set<ProjectScenarioUnit>().AddAsync(newUnit, token);
        }

        await _dbcontext.SaveChangesAsync(token);
        await _dbcontext.CommitTransactionAsync(token);

        return Result.Ok();
    }
}