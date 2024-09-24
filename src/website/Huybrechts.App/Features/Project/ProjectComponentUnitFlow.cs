﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Huybrechts.App.Features.Project.ProjectComponentUnitFlow;

public static class ProjectComponentUnitFlowHelper
{
    public static void CopyFields(Model model, ProjectComponentUnit entity)
    {
        entity.Remark = model.Remark?.Trim();

        entity.Sequence = model.Sequence;
        entity.SetupUnit = model.SetupUnit;
        entity.Variable = model.Variable;
        entity.Category = model.Category;
        entity.Expression = model.Expression;
    }

    public static CreateCommand CreateNew(ProjectComponent component) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectInfoId = component.ProjectInfoId,
        ProjectDesignId = component.ProjectDesignId,
        ProjectComponentId = component.Id,
        ProjectComponent = component
    };

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result DesignNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", id.ToString()));

    internal static Result ComponentNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTCOMPONENT_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTCOMPONENTUNIT_ID.Replace("{0}", id.ToString()));
}

public record Model
{
    public Ulid Id { get; set; }

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Design", ResourceType = typeof(Localization))]
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    [Display(Name = "Component", ResourceType = typeof(Localization))]
    public Ulid ProjectComponentId { get; set; } = Ulid.Empty;

    [Display(Name = "Component", ResourceType = typeof(Localization))]
    public ProjectComponent ProjectComponent { get; set; } = new();

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public Ulid? SetupUnitId { get; set; }

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public SetupUnit? SetupUnit { get; set; } = new();

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Variable), ResourceType = typeof(Localization))]
    public string Variable { get; set; } = string.Empty;

    [Display(Name = nameof(Category), ResourceType = typeof(Localization))]
    public string? Category { get; set; }

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
        RuleFor(m => m.ProjectDesignId).NotNull().NotEmpty();
        RuleFor(m => m.ProjectComponentId).NotNull().NotEmpty();

        RuleFor(m => m.Variable).NotEmpty().Length(1, 128);
        RuleFor(m => m.Category).Length(0, 64);
        RuleFor(m => m.Expression).Length(0, 256);
    }
}

//
// LIST
//

public sealed record ListModel : Model 
{
    [Display(Name = "Component", ResourceType = typeof(Localization))]
    public string ProjectComponentName { get; set; } = string.Empty;

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public string? SetupUnitName { get; set; }
}

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<ProjectComponentUnit, ListModel>()
        .ForMember(dest => dest.ProjectComponentName, opt => opt.MapFrom(src => src.ProjectComponent.Name))
        .ForMember(dest => dest.SetupUnitName, opt => opt.MapFrom(src => (src.SetupUnit ?? new()).Name));
}

public sealed record ListQuery : IRequest<Result<ListResult>>
{
    public Ulid? ProjectComponentId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.ProjectComponentId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult
{
    public Ulid? ProjectComponentId { get; set; } = Ulid.Empty;

    public ProjectInfo ProjectInfo { get; set; } = new();

    public ProjectDesign ProjectDesign { get; set; } = new();

    public ProjectComponent ProjectComponent { get; set; } = new();

    public List<ListModel> Results { get; set; } = [];
}

internal sealed class ListHandler :
    EntityListFlow.Handler<ProjectComponentUnit, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == message.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(message.ProjectComponentId ?? Ulid.Empty);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        IQueryable<ProjectComponentUnit> query = _dbcontext.Set<ProjectComponentUnit>();

        query = query.Where(q => q.ProjectComponentId == message.ProjectComponentId);

        query = query.OrderBy(o => o.Sequence).ThenBy(o => o.Variable);

        var results = await query
            .Include(i => i.ProjectComponent)
            .ProjectTo<ListModel>(_configuration)
            .ToListAsync(token);

        var model = new ListResult
        {
            ProjectComponentId = message.ProjectComponentId,
            ProjectInfo = project,
            ProjectDesign = design,
            ProjectComponent = component,
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
    public Ulid ProjectComponentId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.ProjectComponentId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
{
    public ProjectInfo ProjectInfo { get; set; } = new();

    public ProjectDesign ProjectDesign { get; set; } = new();

    public List<SetupUnit> SetupUnitList { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
{
    public CreateCommandValidator(FeatureContext dbContext) : base()
    {
        RuleFor(x => x.ProjectComponentId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectComponent>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTCOMPONENT_ID.Replace("{0}", m.ProjectComponentId.ToString()));
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
        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == message.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(message.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        var command = ProjectComponentUnitFlowHelper.CreateNew(component);

        command.ProjectInfo = project;
        command.ProjectDesign = design;
        command.ProjectComponent = component;
        command.SetupUnitList = await SetuptUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == message.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(message.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        var setupUnit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q => q.Id == message.SetupUnitId, cancellationToken: token);
        if (setupUnit is null)
        {
            message.SetupUnit = null;
            message.SetupUnitId = Ulid.Empty;
        }
        else
            message.SetupUnit = setupUnit;

        var entity = new ProjectComponentUnit
        {
            Id = message.Id,
            ProjectInfoId = message.ProjectInfoId,
            ProjectDesignId = message.ProjectDesignId,
            ProjectComponent = component,
            CreatedDT = DateTime.UtcNow
        };
        ProjectComponentUnitFlowHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectComponentUnit>().AddAsync(entity, token);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok(entity.Id);
    }
}

//
// CREATE DEFAULT UNITS
//

public sealed record DefaultCommand : IRequest<Result>
{
    public Ulid ProjectComponentId { get; set; } = Ulid.Empty;
}

public sealed class DefaultCommandValidator : AbstractValidator<DefaultCommand>
{
    public DefaultCommandValidator()
    {
        RuleFor(m => m.ProjectComponentId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

internal class DefaultQueryHandler : IRequestHandler<DefaultCommand, Result>
{
    private readonly FeatureContext _dbcontext;
    private readonly IMapper _mapper;

    public DefaultQueryHandler(FeatureContext dbcontext, IMapper mapper)
    {
        _dbcontext = dbcontext;
        _mapper = mapper;
    }

    public async Task<Result> Handle(DefaultCommand message, CancellationToken token)
    {
        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == message.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(message.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        var units = await _dbcontext.Set<ProjectComponentUnit>()
            .Where(q => q.ProjectComponentId == component.Id)
            .Include(i => i.SetupUnit)
            .ToListAsync(token);
        var index = units.Count * 10 + 10;

        if (component.SourceType == SourceType.Platform) 
        {
            if (component.PlatformInfoId.HasValue && component.PlatformInfoId != Ulid.Empty)
            {
                var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(f => f.Id == component.PlatformInfoId, token);
                if(platform is not null && component.PlatformProductId.HasValue && component.PlatformProductId != Ulid.Empty)
                {
                    var product = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(f => f.Id == component.PlatformProductId, token);
                    if (product is not null)
                    {
                        var rates = await _dbcontext.Set<PlatformRate>()
                            .Where(q => q.PlatformProductId == product.Id)
                            .Include(i => i.RateUnits)
                            .ThenInclude(j => j.SetupUnit)
                            .ToListAsync(token);

                        await _dbcontext.BeginTransactionAsync(token);

                        if (rates is not null && rates.Count > 0)
                        {
                            // Get all SetupUnit IDs associated with the current component units
                            var existingUnitIds = units.Select(u => u.SetupUnit!.Id).ToHashSet();

                            // Get all SetupUnits from the rates
                            var allSetupUnitsFromRates = rates.SelectMany(r => r.RateUnits)
                                .Select(ru => ru.SetupUnit)
                                .Distinct()
                                .ToList();

                            foreach (var rate in rates)
                            {
                                foreach (var rateUnit in rate.RateUnits)
                                {
                                    if (existingUnitIds.Contains(rateUnit.SetupUnit.Id))
                                        continue;

                                    ProjectComponentUnit newComponentUnit = new()
                                    {
                                        Id = Ulid.NewUlid(),
                                        CreatedDT = DateTime.UtcNow,
                                        ProjectInfoId = project.Id,
                                        ProjectDesignId = design.Id,
                                        ProjectComponent = component,

                                        Sequence = index,
                                        SetupUnit = rateUnit.SetupUnit,
                                        Variable = rateUnit.SetupUnit.Name.ToLower().Trim(),
                                        Expression = "1"
                                    };

                                    await _dbcontext.Set<ProjectComponentUnit>().AddAsync(newComponentUnit, token);
                                    existingUnitIds.Add(rateUnit.SetupUnit.Id);
                                    index += 10;
                                }
                            }
                        }

                        await _dbcontext.SaveChangesAsync(token);
                        await _dbcontext.CommitTransactionAsync(token);
                    }
                }
            }
        }

        return Result.Ok();
    }
}

//TODO create default units

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

    public ProjectDesign ProjectDesign { get; set; } = new();

    public List<SetupUnit> SetupUnitList { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.ProjectComponentId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectComponent>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTCOMPONENT_ID.Replace("{0}", m.ProjectComponentId.ToString()));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectComponentUnit, UpdateCommand>();
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
        var command = await _dbcontext.Set<ProjectComponentUnit>()
            .Include(i => i.ProjectComponent)
            .Include(i => i.SetupUnit)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectComponentUnitFlowHelper.EntityNotFound(message.Id);

        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == command.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(command.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        command.ProjectInfo = project;
        command.ProjectDesign = design;
        command.ProjectComponent = component;
        command.SetupUnitList = await SetuptUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        var entity = await _dbcontext.Set<ProjectComponentUnit>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectComponentUnitFlowHelper.EntityNotFound(message.Id);

        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == entity.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(entity.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        var setupUnit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q => q.Id == message.SetupUnitId, cancellationToken: token);
        if (setupUnit is null)
        {
            message.SetupUnit = null;
            message.SetupUnitId = Ulid.Empty;
        }
        else
            message.SetupUnit = setupUnit;

        entity.ModifiedDT = DateTime.UtcNow;
        ProjectComponentUnitFlowHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectComponentUnit>().Update(entity);
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

    public ProjectDesign ProjectDesign { get; set; } = new();

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
        CreateProjection<ProjectComponentUnit, DeleteCommand>()
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
        var command = await _dbcontext.Set<ProjectComponentUnit>()
            .Include(i => i.ProjectComponent)
            .Include(i => i.SetupUnit)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectComponentUnitFlowHelper.EntityNotFound(message.Id);

        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == command.ProjectComponentId, cancellationToken: token);
        if (component == null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(command.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        command.ProjectInfo = project;
        command.ProjectDesign = design;
        command.ProjectComponent = component;

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
        var entity = await _dbcontext.Set<ProjectComponentUnit>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectComponentUnitFlowHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectComponentUnit>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}