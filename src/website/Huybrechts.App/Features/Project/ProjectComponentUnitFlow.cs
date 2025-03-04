using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Project.ProjectQuantityFlow;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Huybrechts.App.Features.Project.ProjectComponentUnitFlow;

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

    [Display(Name = "BillOfQuantity", ResourceType = typeof(Localization))]
    public Ulid? ProjectQuantityId { get; set; }

    [Display(Name = "BillOfQuantity", ResourceType = typeof(Localization))]
    public ProjectQuantity? ProjectQuantity { get; set; }

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(Variable), ResourceType = typeof(Localization))]
    public string? Variable { get; set; }

    [Display(Name = nameof(Category), ResourceType = typeof(Localization))]
    public string? Category { get; set; }

    [Display(Name = nameof(Expression), ResourceType = typeof(Localization))]
    public string? Expression { get; set; }

    [Precision(18, 6)]
    [Display(Name = nameof(Quantity), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
    public decimal Quantity { get; set; }

    [Precision(18, 6)]
    [Display(Name = nameof(SalesPrice), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
    public decimal SalesPrice { get; set; }

    [Precision(18, 6)]
    [Display(Name = nameof(RetailPrice), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
    public decimal RetailPrice { get; set; }

    [Precision(18, 6)]
    [Display(Name = nameof(UnitPrice), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
    public decimal UnitPrice { get; set; }

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

        RuleFor(m => m.Description).Length(0, 256);
        RuleFor(m => m.Variable).Length(0, 128);
        RuleFor(m => m.Category).Length(0, 64);
        RuleFor(m => m.Expression).Length(0, 256);
    }
}

public static class ProjectComponentUnitFlowHelper
{
    public static void CopyFields(Model model, ProjectComponentUnit entity)
    {
        entity.ProjectQuantity = model.ProjectQuantity;
        if (model.ProjectQuantity is null)
            entity.ProjectQuantityId = null;
        entity.SetupUnit = model.SetupUnit;
        if (model.SetupUnit is null)
            entity.SetupUnitId = null;
        entity.Remark = model.Remark?.Trim();

        entity.Sequence = model.Sequence;
        entity.Description = model.Description;
        entity.Variable = model.Variable;
        entity.Category = model.Category;
        entity.Expression = model.Expression;
        entity.Quantity = model.Quantity;
        entity.SalesPrice = model.SalesPrice;
        entity.RetailPrice = model.RetailPrice;
        entity.UnitPrice = model.UnitPrice;
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

//
// LIST
//

public sealed record ListModel : Model 
{
    [Display(Name = "Component", ResourceType = typeof(Localization))]
    public string ProjectComponentName { get; set; } = string.Empty;

    [Display(Name = "Unit", ResourceType = typeof(Localization))]
    public string? SetupUnitName { get; set; }

    [Display(Name = "BillOfQuantity", ResourceType = typeof(Localization))]
    public string? ProjectQuantityName { get; set; }
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
    EntityFlow.ListHandler<ProjectComponentUnit, ListModel>,
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
            .Include(i => i.ProjectQuantity)
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

    public List<ProjectQuantity> ProjectQuantityList { get; set; } = [];
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
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public CreateQueryHandler(FeatureContext dbcontext, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _dbcontext = dbcontext;
        _cache = cache;
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
        command.SetupUnitList = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token);
        command.ProjectQuantityList = await ProjectQuantityHelper.GetBillOfQuantitiesAsync(_dbcontext, token);

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
        var component = await _dbcontext.Set<ProjectComponent>().FirstOrDefaultAsync(q => q.Id == message.ProjectComponentId, token);
        if (component is null)
            return ProjectComponentUnitFlowHelper.ComponentNotFound(message.ProjectComponentId);

        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == component.ProjectDesignId, token);
        if (design is null)
            return ProjectComponentUnitFlowHelper.DesignNotFound(component.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == component.ProjectInfoId, token);
        if (project is null)
            return ProjectComponentUnitFlowHelper.ProjectNotFound(component.ProjectInfoId);

        var setupUnit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q => q.Id == message.SetupUnitId, token);
        if (setupUnit is not null)
        {
            message.SetupUnit = setupUnit;
        }
        else
            message.SetupUnit = null;

        var quantity = await _dbcontext.Set<ProjectQuantity>().FirstOrDefaultAsync(q => q.Id == message.ProjectQuantityId, token);
        if (quantity is not null)
        {
            message.ProjectQuantity = quantity;
        }
        else
            message.ProjectQuantity = null;

        ProjectComponentUnit entity = new()
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

    public DefaultQueryHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
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

        await _dbcontext.BeginTransactionAsync(token);

        if (component.SourceType == SourceType.Platform)
        {
            await HandlePlatformAsync(component, index, token);
        }

        await _dbcontext.CommitTransactionAsync(token);
        return Result.Ok();
    }

    private async Task HandlePlatformAsync(ProjectComponent component, int index, CancellationToken token)
    {
        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(f => f.Id == component.PlatformInfoId, token);
        if (platform is null)
            return;

        var product = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(f => f.Id == component.PlatformProductId, token);
        if (product is null)
            return;

        var rates = await _dbcontext.Set<PlatformRate>()
            .Where(q => q.PlatformProductId == product.Id)
            .Include(i => i.RateUnits)
            .ThenInclude(j => j.SetupUnit)
            .ToListAsync(token);
        if (rates is null || rates.Count == 0)
            return;

        var itemList = rates
            .Select(i => new { i.ServiceName, i.ProductName, i.SkuName })
            .Distinct()
            .ToList();
        if (itemList is null || itemList.Count == 0)
            return;

        foreach(var item in itemList)
        {
            var defaultUnits = await _dbcontext.Set<PlatformDefaultUnit>()
                .Include(i => i.SetupUnit)
                .Where(unit => unit.PlatformInfoId == component.PlatformInfoId
                && (unit.ServiceName != null && unit.ServiceName.ToLower() == item.ServiceName.ToLower())
                && (unit.ProductName != null && unit.ProductName.ToLower() == item.ProductName.ToLower())
                && (unit.SkuName != null && unit.SkuName.ToLower() == item.SkuName.ToLower())
                && (unit.IsDefaultProjectComponentUnit == true))
                .OrderBy(unit => unit.ServiceName)
                .ThenBy(unit => unit.ProductName)
                .ThenBy(unit => unit.SkuName)
                .ThenBy(unit => unit.Sequence)
                .ToListAsync(token);

            if (defaultUnits is null || defaultUnits.Count == 0)
                continue;

            foreach(var dftUnit in defaultUnits)
            {
                ProjectComponentUnit newUnit = new()
                {
                    Id = Ulid.NewUlid(),
                    CreatedDT = DateTime.UtcNow,
                    ProjectInfoId = component.ProjectInfoId,
                    ProjectDesignId = component.ProjectDesignId,
                    ProjectComponent = component,
                    Sequence = index,
                    Description = dftUnit.Description,
                    SetupUnit = dftUnit.SetupUnit,
                    SetupUnitId = dftUnit.SetupUnitId,
                    Category = string.Empty,
                    Variable = dftUnit.Variable,
                    Quantity = dftUnit.DefaultValue,
                    Expression = dftUnit.Expression
                };
                await _dbcontext.Set<ProjectComponentUnit>().AddAsync(newUnit, token);
                index += 10;
            }
        }
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

    public ProjectDesign ProjectDesign { get; set; } = new();

    public List<SetupUnit> SetupUnitList { get; set; } = [];

    public List<ProjectQuantity> ProjectQuantityList { get; set; } = [];
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
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public UpdateQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _dbcontext = dbcontext;
        _configuration = configuration;
        _cache = cache;
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
        command.SetupUnitList = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token);
        command.ProjectQuantityList = await ProjectQuantityHelper.GetBillOfQuantitiesAsync(_dbcontext, token);

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
        {
            message.SetupUnit = setupUnit;
            message.SetupUnitId = setupUnit.Id;
        }

        var quantity = await _dbcontext.Set<ProjectQuantity>().FirstOrDefaultAsync(q => q.Id == message.ProjectQuantityId, cancellationToken: token);
        if (quantity is null)
        {
            message.ProjectQuantity = null;
            message.ProjectQuantityId = Ulid.Empty;
        }
        else
        {
            message.ProjectQuantity = quantity;
            message.ProjectQuantityId = quantity.Id;
        }

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

    public string ProjectQuantityName { get; set; } = string.Empty;
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
        .ForMember(dest => dest.SetupUnitName, opt => opt.MapFrom(src => (src.SetupUnit ?? new()).Name))
        .ForMember(dest => dest.ProjectQuantityName, opt => opt.MapFrom(src => (src.ProjectQuantity ?? new()).Name));
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
            .Include(i => i.ProjectQuantity)
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