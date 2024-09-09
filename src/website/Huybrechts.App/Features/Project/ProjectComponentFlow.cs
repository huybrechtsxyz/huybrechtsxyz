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
    public static CreateCommand CreateNew(ProjectDesign design) => new()
    {
        Id = Ulid.NewUlid(),
        ProjectDesign = design,
        ProjectDesignId = design.Id
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
        entity.Level = model.Level;
        entity.Variant = model.Variant;
        entity.SourceType = model.SourceType;
        entity.Source = model.Source?.Trim();
        entity.PlatformInfoId = model.PlatformInfoId;
        entity.PlatformProductId = model.PlatformProductId;
    }

    internal static Result ProjectNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECT_ID.Replace("{0}", id.ToString()));

    internal static Result DesignNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTDESIGN_ID.Replace("{0}", id.ToString()));

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_PROJECTCOMPONENT_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateEntityFound(string name) => Result.Fail(Messages.DUPLICATE_PROJECTCOMPONENT_NAME.Replace("{0}", name.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(
        DbContext context, 
        string name, 
        Ulid ProjectInfoId, 
        Ulid? parentId = null,
        Ulid? currentId = null)
    {
        name = name.ToLower().Trim();
        return await context.Set<ProjectComponent>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                            && pr.ProjectInfoId == ProjectInfoId
                            && (!parentId.HasValue || pr.ParentId != parentId.Value)
                            && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

public record Model
{
    public Ulid Id { get; set; }

    [Display(Name = "Project", ResourceType = typeof(Localization))]
    public Ulid ProjectInfoId { get; set; } = Ulid.Empty;

    public Ulid ProjectDesignId { get; set; } = Ulid.Empty;

    public Ulid? ParentId { get; set; } = Ulid.Empty;

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    [Display(Name = "Components", ResourceType = typeof(Localization))]
    public List<ProjectComponent> SubComponents { get; set; } = [];

    [Display(Name = nameof(ComponentLevel), ResourceType = typeof(Localization))]
    public ComponentLevel Level { get; set; } = ComponentLevel.Component;

    [Display(Name = nameof(VariantType), ResourceType = typeof(Localization))]
    public VariantType Variant { get; set; } = VariantType.Standard;

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

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<ProjectComponent, ListModel>();
}

public sealed record ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>>
{
    public Ulid? ProjectDesignId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.ProjectDesignId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult : EntityListFlow.Result<ListModel>
{
    public Ulid? ProjectDesignId { get; set; } = Ulid.Empty;

    public ProjectInfo ProjectInfo = new();

    public ProjectDesign ProjectDesign = new();
}

internal sealed class ListHandler :
    EntityListFlow.Handler<ProjectComponent, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
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

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            string searchFor = searchString.ToLowerInvariant();
            query = query.Where(q =>
                q.ProjectInfoId == project.Id
                && q.ProjectDesignId == design.Id
                && (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)));
        }
        else
        {
            query = query.Where(q => q.ProjectInfoId == project.Id && q.ProjectDesignId == design.Id);
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
            ProjectDesignId = message.ProjectDesignId,
            ProjectInfo = project,
            ProjectDesign = design,
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

        RuleFor(x => x).MustAsync(async (model, cancellation) =>
        {
            bool exists = await ProjectComponentHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTCOMPONENT_NAME.Replace("{0}", x.Name.ToString()))
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

        var command = ProjectComponentHelper.CreateNew(design);
        
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

        if (await ProjectComponentHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId))
            return ProjectComponentHelper.DuplicateEntityFound(message.Name);

        var entity = new ProjectComponent
        {
            Id = message.Id,
            ProjectInfoId = design.ProjectInfoId,
            ProjectDesign = design,
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

        RuleFor(x => x).MustAsync(async (model, cancellation) =>
        {
            bool exists = await ProjectComponentHelper.IsDuplicateNameAsync(dbContext, model.Name, model.ProjectInfoId, model.Id);
            return !exists;
        })
        .WithMessage(x => Messages.DUPLICATE_PROJECTCOMPONENT_NAME.Replace("{0}", x.Name.ToString()))
        .WithName(nameof(CreateCommand.Name));
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

        if (await ProjectComponentHelper.IsDuplicateNameAsync(_dbcontext, message.Name, message.ProjectInfoId, entity.Id))
            return ProjectComponentHelper.DuplicateEntityFound(message.Name);

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