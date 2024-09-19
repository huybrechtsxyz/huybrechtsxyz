using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Hangfire;
using Huybrechts.App.Application;
using Huybrechts.App.Data;
using Huybrechts.Core.Project;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project.ProjectSimulationFlow;

public static class ProjectSimulationHelper
{
    public static string GetSearchIndex
        (string name, string? description, string? tags)
        => $"{name}~{description}~{tags}".ToLowerInvariant();

    public static void CopyFields(Model model, ProjectSimulation entity)
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

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTSIMULATION_ID.Replace("{0}", id.ToString()));
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

    [Display(Name = nameof(IsCalculating), ResourceType = typeof(Localization))]
    public bool IsCalculating { get; set; } = false;

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    [Display(Name = nameof(Tags), ResourceType = typeof(Localization))]
    public string? Tags { get; set; }

    public string SearchIndex => ProjectSimulationHelper.GetSearchIndex(Name, Description, Tags);
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
        CreateProjection<ProjectSimulation, ListModel>()
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
    EntityListFlow.Handler<ProjectSimulation, ListModel>,
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
            return ProjectSimulationHelper.ProjectNotFound(message.ProjectInfoId ?? Ulid.Empty);

        IQueryable<ProjectSimulation> query = _dbcontext.Set<ProjectSimulation>();

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
// CALC
//

public sealed record CalculationQuery : IRequest<Result> 
{
    public string TenantId { get; init; } = string.Empty;

    public Ulid ProjectSimulationId { get; init; }
}

public class CalculationValidator : AbstractValidator<CalculationQuery>
{
    public CalculationValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.ProjectSimulationId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectSimulation>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTSIMULATION_ID.Replace("{0}", m.ProjectSimulationId.ToString()));
    }
}

internal class CalculationHandler : IRequestHandler<CalculationQuery, Result>
{
    private readonly FeatureContext _dbcontext;

    public CalculationHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<Result> Handle(CalculationQuery message, CancellationToken token)
    {
        BackgroundJob.Enqueue<CalculateSimulationWorker>(x => x.StartAsync(message.TenantId, message.ProjectSimulationId, token));

        return Result.Ok();
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
            return ProjectSimulationHelper.ProjectNotFound(message.ProjectInfoId);

        var command = ProjectSimulationHelper.CreateNew(Project);

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
            return ProjectSimulationHelper.ProjectNotFound(message.ProjectInfoId);

        var entity = new ProjectSimulation
        {
            Id = message.Id,
            ProjectInfo = Project,
            CreatedDT = DateTime.UtcNow
        };
        ProjectSimulationHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectSimulation>().AddAsync(entity, token);
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
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectSimulation, UpdateCommand>()
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
        var command = await _dbcontext.Set<ProjectSimulation>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectSimulationHelper.EntityNotFound(message.Id);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectSimulationHelper.ProjectNotFound(command.ProjectInfoId);

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
        var entity = await _dbcontext.Set<ProjectSimulation>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectSimulationHelper.EntityNotFound(message.Id);

        entity.ModifiedDT = DateTime.UtcNow;
        ProjectSimulationHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectSimulation>().Update(entity);
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
        CreateProjection<ProjectSimulation, DeleteCommand>()
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
        var command = await _dbcontext.Set<ProjectSimulation>()
            .Include(i => i.ProjectInfo)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectSimulationHelper.EntityNotFound(message.Id);

        var Project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (Project is null)
            return ProjectSimulationHelper.ProjectNotFound(command.ProjectInfoId);

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
        var entity = await _dbcontext.Set<ProjectSimulation>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectSimulationHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectSimulation>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}