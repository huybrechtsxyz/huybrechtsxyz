using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Project;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Project.ProjectComponentFlow;

public static class ProjectComponentHelper
{
    public static CreateCommand CreateNew(ProjectInfo project, ProjectDesign design, ProjectComponent? parent) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectInfo = project,
        ProjectInfoId = project.Id,
        ProjectDesign = design,
        ProjectDesignId = design.Id,
        ParentInfo = parent,
        ParentId = parent?.Id,
    };

    public static string GetSearchIndex
        (string name, string? description, string? source)
        => $"{name}~{description}~{source}".ToLowerInvariant();

    public static void CopyFields(Model model, ProjectComponent entity)
    {
        entity.Name = model.Name.Trim();
        entity.Description = model.Description?.Trim();
        entity.Remark = model.Remark?.Trim();
        entity.SearchIndex = model.SearchIndex;

        entity.Sequence = model.Sequence;
        entity.ComponentLevel = model.ComponentLevel;
        entity.VariantType = model.VariantType;
        entity.SourceType = model.SourceType;
        entity.Source = model.Source?.Trim();
        entity.PlatformInfoId = model.PlatformInfoId;
        entity.PlatformProductId = model.PlatformProductId;
    }

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result DesignNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTCOMPONENT_ID.Replace("{0}", id.ToString()));
}

public record Model
{
    public Ulid Id { get; set; }

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Design", ResourceType = typeof(Localization))]
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    [Display(Name = "Component", ResourceType = typeof(Localization))]
    public Ulid? ParentId { get; set; } = Ulid.Empty;

    [Display(Name = "Components", ResourceType = typeof(Localization))]
    public List<Model> Children { get; set; } = [];

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    [Display(Name = nameof(ComponentLevel), ResourceType = typeof(Localization))]
    public ComponentLevel ComponentLevel { get; set; } = ComponentLevel.Component;

    [Display(Name = nameof(VariantType), ResourceType = typeof(Localization))]
    public VariantType VariantType { get; set; } = VariantType.Standard;

    [Display(Name = nameof(SourceType), ResourceType = typeof(Localization))]
    public SourceType SourceType { get; set; } = SourceType.None;

    [DataType(DataType.Url)]
    [Display(Name = nameof(Source), ResourceType = typeof(Localization))]
    public string? Source { get; set; }

    [DataType(DataType.Url)]
    [Display(Name = "Platform", ResourceType = typeof(Localization))]
    public Ulid? PlatformInfoId { get; set; }

    [Display(Name = "PlatformProduct", ResourceType = typeof(Localization))]
    public Ulid? PlatformProductId { get; set; }

    public string SearchIndex => ProjectComponentHelper.GetSearchIndex(Name, Description, Source);
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotNull().NotEmpty();
        RuleFor(m => m.ProjectInfoId).NotNull().NotEmpty();
        RuleFor(m => m.ProjectDesignId).NotNull().NotEmpty();
        RuleFor(m => m.Name).NotEmpty().Length(1, 128);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

//
// LIST
//

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateMap<ProjectComponent, Model>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));
}

internal sealed class EntityMapping : Profile
{
    public EntityMapping() =>
        CreateMap<ProjectComponent, ProjectComponent>()
        .ForMember(dest => dest.Children, opt => opt.Ignore());
}

public sealed record ListQuery : IRequest<Result<ListResult>>
{
    public Ulid? ProjectDesignId { get; set; } = Ulid.Empty;

    public Ulid? ParentId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.ProjectDesignId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult
{
    public Ulid? ProjectDesignId { get; set; } = Ulid.Empty;

    public Ulid? ParentId { get; set; } = Ulid.Empty;

    public ProjectInfo ProjectInfo = new();

    public ProjectDesign ProjectDesign = new();

    public List<Model> Results = [];
}

internal sealed class ListHandler :
    EntityListFlow.Handler<ProjectComponent, Model>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    private readonly IMapper _mapper;

    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration, IMapper mapper)
        : base(dbcontext, configuration)
    {
        _mapper = mapper;
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        var design = await _dbcontext.Set<ProjectDesign>().FirstOrDefaultAsync(q => q.Id == message.ProjectDesignId, cancellationToken: token);
        if (design == null)
            return ProjectComponentHelper.ProjectNotFound(message.ProjectDesignId ?? Ulid.Empty);

        var project = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(q => q.Id == design.ProjectInfoId, cancellationToken: token);
        if (project == null)
            return ProjectComponentHelper.ProjectNotFound(design.ProjectInfoId);

        IQueryable<ProjectComponent> query = _dbcontext.Set<ProjectComponent>();
        query = query.Where(q => q.ProjectInfoId == project.Id && q.ProjectDesignId == design.Id);
        if (message.ParentId.HasValue)
            query = query.Where(q => q.ParentId == message.ParentId);
        query = query.OrderBy(o => o.Sequence).ThenBy(o => o.Name);

        List<ProjectComponent> recordList = await query.ToListAsync(cancellationToken: token) ?? [];

        List<ProjectComponent> BuildHierarchy(Ulid? parentId)
        {
            return recordList
                .Where(q => q.ParentId == parentId)
                .Select(s =>
                {
                    var mappedComponent = _mapper.Map<ProjectComponent>(s);
                    mappedComponent.Children = BuildHierarchy(s.Id);
                    return mappedComponent;
                })
                .ToList();
        }

        var records = BuildHierarchy(Ulid.Empty);

        var results = _mapper.Map<List<Model>>(records);

        var model = new ListResult
        {
            ProjectDesignId = message.ProjectDesignId,
            ParentId = message.ParentId,
            ProjectInfo = project,
            ProjectDesign = design,
            Results = results
        };

        return model;
    }
}

//
// CREATE
//

public sealed record CreateQuery : IRequest<Result<CreateCommand>>
{
    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    public Ulid? ParentId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.ProjectDesignId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
{
    public ProjectInfo ProjectInfo { get; set; } = new();

    public ProjectDesign ProjectDesign { get; set; } = new();

    public ProjectComponent? ParentInfo { get; set; } = new();
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

        RuleFor(x => x.ProjectDesignId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectDesign>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", m.ProjectDesignId.ToString()));
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
        var design = await _dbcontext.Set<ProjectDesign>().FindAsync([message.ProjectDesignId], cancellationToken: token);
        if (design is null)
            return ProjectComponentHelper.DesignNotFound(message.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([design.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectComponentHelper.ProjectNotFound(design.ProjectInfoId);

        ProjectComponent? parent = null;
        if (message.ParentId.HasValue && message.ParentId != Ulid.Empty)
        {
            parent = await _dbcontext.Set<ProjectComponent>().FindAsync([message.ParentId], cancellationToken: token);
            if (parent is null)
                return ProjectComponentHelper.EntityNotFound(message.ParentId ?? Ulid.Empty);
        }

        var command = ProjectComponentHelper.CreateNew(project, design, parent);
        
        command.ProjectInfo = project;
        command.ParentInfo = parent;

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
        var design = await _dbcontext.Set<ProjectDesign>().FindAsync([message.ProjectDesignId], cancellationToken: token);
        if (design is null)
            return ProjectComponentHelper.DesignNotFound(message.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([design.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectComponentHelper.ProjectNotFound(design.ProjectInfoId);

        ProjectComponent? parent = null;
        if (message.ParentId.HasValue && message.ParentId != Ulid.Empty)
        {
            parent = await _dbcontext.Set<ProjectComponent>().FindAsync([message.ParentId], cancellationToken: token);
            if (parent is null)
                return ProjectComponentHelper.EntityNotFound(message.ParentId ?? Ulid.Empty);
        }

        var entity = new ProjectComponent
        {
            Id = message.Id,
            ProjectInfoId = project.Id,
            ProjectDesign = design,
            ParentId = parent?.Id ?? Ulid.Empty,
            CreatedDT = DateTime.UtcNow
        };
        ProjectComponentHelper.CopyFields(message, entity);
            
        await _dbcontext.Set<ProjectComponent>().AddAsync(entity, token);
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

    public ProjectDesign ProjectDesign { get; set; } = new();

    public ProjectComponent? ParentInfo { get; set; }
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

        RuleFor(x => x.ProjectDesignId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<ProjectInfo>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", m.ProjectDesignId.ToString()));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<ProjectComponent, UpdateCommand>();
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
        var command = await _dbcontext.Set<ProjectComponent>()
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null) 
            return ProjectComponentHelper.EntityNotFound(message.Id);

        var design = await _dbcontext.Set<ProjectDesign>().FindAsync([command.ProjectDesignId], cancellationToken: token);
        if (design is null)
            return ProjectComponentHelper.DesignNotFound(command.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectComponentHelper.ProjectNotFound(command.ProjectInfoId);

        ProjectComponent? parent = null;
        if (command.ParentId is not null && command.ParentId != Ulid.Empty)
        {
            parent = await _dbcontext.Set<ProjectComponent>().FindAsync([command.ParentId], cancellationToken: token);
        }

        command.ProjectInfo = project;
        command.ProjectDesign = design;
        command.ParentInfo = parent;

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
        var entity = await _dbcontext.Set<ProjectComponent>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectComponentHelper.EntityNotFound(message.Id);

        entity.ModifiedDT = DateTime.UtcNow;
        ProjectComponentHelper.CopyFields(message, entity);
            
        _dbcontext.Set<ProjectComponent>().Update(entity);
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

    public ProjectComponent? ParentInfo { get; set; }
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
        CreateProjection<ProjectComponent, DeleteCommand>();
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
        var command = await _dbcontext.Set<ProjectComponent>()
            .Include(i => i.ProjectDesign)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (command == null)
            return ProjectComponentHelper.EntityNotFound(message.Id);

        var design = await _dbcontext.Set<ProjectDesign>().FindAsync([command.ProjectDesignId], cancellationToken: token);
        if (design is null)
            return ProjectComponentHelper.DesignNotFound(command.ProjectDesignId);

        var project = await _dbcontext.Set<ProjectInfo>().FindAsync([command.ProjectInfoId], cancellationToken: token);
        if (project is null)
            return ProjectComponentHelper.ProjectNotFound(command.ProjectInfoId);

        ProjectComponent? parent = null;
        if (command.ParentId is not null && command.ParentId != Ulid.Empty)
        {
            parent = await _dbcontext.Set<ProjectComponent>().FindAsync([command.ParentId], cancellationToken: token);
        }

        command.ProjectInfo = project;
        command.ProjectDesign = design;
        command.ParentInfo = parent;

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
        var entity = await _dbcontext.Set<ProjectComponent>().FindAsync([message.Id], cancellationToken: token);
        if (entity is null)
            return ProjectComponentHelper.EntityNotFound(message.Id);

        _dbcontext.Set<ProjectComponent>().Remove(entity);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}