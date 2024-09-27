using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project.ProjectScenarioUnitFlow;

public static class ProjectScenarioUnitFlowHelper
{
    public static void CopyFields(Model model, ProjectScenarioUnit entity)
    {
        entity.Remark = model.Remark?.Trim();

        entity.Sequence = model.Sequence;
        entity.SetupUnit = model.SetupUnit;
        entity.Variable = model.Variable;
        entity.Expression = model.Expression;
    }

    public static CreateCommand CreateNew(ProjectScenario Scenario) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectInfoId = Scenario.ProjectInfoId,
        ProjectScenarioId = Scenario.Id,
        ProjectScenario = Scenario
    };

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result ScenarioNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTSCENARIO_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTSCENARIOUNIT_ID.Replace("{0}", id.ToString()));
}

public record Model
{
    public Ulid Id { get; set; }

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Scenario", ResourceType = typeof(Localization))]
    public Ulid ProjectScenarioId { get; set; } = Ulid.Empty;

    [Display(Name = "Scenario", ResourceType = typeof(Localization))]
    public ProjectScenario ProjectScenario { get; set; } = new();

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public Ulid? SetupUnitId { get; set; }

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public SetupUnit? SetupUnit { get; set; } = new();

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Variable), ResourceType = typeof(Localization))]
    public string Variable { get; set; } = string.Empty;

    [Display(Name = nameof(Expression), ResourceType = typeof(Localization))]
    public string? Expression { get; set; }

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty();
        RuleFor(m => m.ProjectInfoId).NotNull().NotEmpty();
        RuleFor(m => m.ProjectScenarioId).NotNull().NotEmpty();
        RuleFor(m => m.Variable).NotEmpty().Length(1, 128);
        RuleFor(m => m.Expression).Length(0, 256);
    }
}

//
// LIST
//

public sealed record ListModel : Model 
{
    [Display(Name = "Scenario", ResourceType = typeof(Localization))]
    public string ProjectScenarioName { get; set; } = string.Empty;

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public string? SetupUnitName { get; set; }
}

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<ProjectScenarioUnit, ListModel>()
        .ForMember(dest => dest.ProjectScenarioName, opt => opt.MapFrom(src => src.ProjectScenario.Name))
        .ForMember(dest => dest.SetupUnitName, opt => opt.MapFrom(src => (src.SetupUnit ?? new()).Name));
}

public sealed record ListQuery : IRequest<Result<ListResult>>
{
    public Ulid? ProjectScenarioId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.ProjectScenarioId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult
{
    public Ulid? ProjectScenarioId { get; set; } = Ulid.Empty;

    public ProjectInfo ProjectInfo { get; set; } = new();

    public ProjectScenario ProjectScenario { get; set; } = new();

    public List<ListModel> Results { get; set; } = [];
}

internal sealed class ListHandler :
    EntityFlow.ListHandler<ProjectScenarioUnit, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == message.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(message.ProjectScenarioId ?? Ulid.Empty);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        IQueryable<ProjectScenarioUnit> query = _dbcontext.Set<ProjectScenarioUnit>();

        query = query.Where(q => q.ProjectScenarioId == message.ProjectScenarioId);

        query = query.OrderBy(o => o.Sequence).ThenBy(o => o.Variable);

        var results = await query
            .Include(i => i.ProjectScenario)
            .ProjectTo<ListModel>(_configuration)
            .ToListAsync(token);

        var model = new ListResult
        {
            ProjectScenarioId = message.ProjectScenarioId,
            ProjectInfo = project,
            ProjectScenario = Scenario,
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
    public Ulid ProjectScenarioId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.ProjectScenarioId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
{
    public ProjectInfo ProjectInfo { get; set; } = new();

    public List<SetupUnit> SetupUnitList { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
{
    public CreateCommandValidator(FeatureContext dbContext) : base()
    {
        RuleFor(x => x.ProjectScenarioId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectScenario>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTSCENARIO_ID.Replace("{0}", m.ProjectScenarioId.ToString()));
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
        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == message.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(message.ProjectScenarioId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        var command = ProjectScenarioUnitFlowHelper.CreateNew(Scenario);

        command.ProjectInfo = project;
        command.ProjectScenario = Scenario;
        command.SetupUnitList = await SetupUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == message.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(message.ProjectScenarioId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        var setupUnit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q => q.Id == message.SetupUnitId, cancellationToken: token);
        if (setupUnit is null)
        {
            message.SetupUnit = null;
            message.SetupUnitId = Ulid.Empty;
        }
        else
            message.SetupUnit = setupUnit;

        var entity = new ProjectScenarioUnit
        {
            Id = message.Id,
            ProjectInfoId = message.ProjectInfoId,
            ProjectScenario = Scenario,
            CreatedDT = DateTime.UtcNow
        };
        ProjectScenarioUnitFlowHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectScenarioUnit>().AddAsync(entity, token);
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

    public List<SetupUnit> SetupUnitList { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.ProjectScenarioId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectScenario>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTSCENARIO_ID.Replace("{0}", m.ProjectScenarioId.ToString()));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectScenarioUnit, UpdateCommand>();
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
        var command = await _dbcontext.Set<ProjectScenarioUnit>()
            .Include(i => i.ProjectScenario)
            .Include(i => i.SetupUnit)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectScenarioUnitFlowHelper.EntityNotFound(message.Id);

        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == command.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(command.ProjectScenarioId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        command.ProjectInfo = project;
        command.ProjectScenario = Scenario;
        command.SetupUnitList = await SetupUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        var entity = await _dbcontext.Set<ProjectScenarioUnit>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectScenarioUnitFlowHelper.EntityNotFound(message.Id);

        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == entity.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(entity.ProjectScenarioId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        var setupUnit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q => q.Id == message.SetupUnitId, cancellationToken: token);
        if (setupUnit is null)
        {
            message.SetupUnit = null;
            message.SetupUnitId = Ulid.Empty;
        }
        else
            message.SetupUnit = setupUnit;

        entity.ModifiedDT = DateTime.UtcNow;
        ProjectScenarioUnitFlowHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectScenarioUnit>().Update(entity);
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

    public string SetupUnitName { get; set; } = string.Empty;
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
        CreateProjection<ProjectScenarioUnit, DeleteCommand>()
        .ForMember(dest => dest.SetupUnitName, opt => opt.MapFrom(src => (src.SetupUnit ?? new()).Name));
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
        var command = await _dbcontext.Set<ProjectScenarioUnit>()
            .Include(i => i.ProjectScenario)
            .Include(i => i.SetupUnit)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectScenarioUnitFlowHelper.EntityNotFound(message.Id);

        var Scenario = await _dbcontext.Set<ProjectScenario>().FirstOrDefaultAsync(q => q.Id == command.ProjectScenarioId, cancellationToken: token);
        if (Scenario == null)
            return ProjectScenarioUnitFlowHelper.ScenarioNotFound(command.ProjectScenarioId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == Scenario.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectScenarioUnitFlowHelper.ProjectNotFound(Scenario.ProjectInfoId);

        command.ProjectInfo = project;
        command.ProjectScenario = Scenario;

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
        var entity = await _dbcontext.Set<ProjectScenarioUnit>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectScenarioUnitFlowHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectScenarioUnit>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}

