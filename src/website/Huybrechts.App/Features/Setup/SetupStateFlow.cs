using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Huybrechts.App.Features.Setup.SetupStateFlow;

public record Model
{
    public Ulid Id { get; init; }

    [Display(Name = nameof(TypeOf), ResourceType = typeof(Localization))]
    public string TypeOf { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Display(Name = nameof(StateType), ResourceType = typeof(Localization))]
    public StateType StateType { get; set; }

    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    public string SearchIndex => $"{TypeOf}~{Name}~{Description}".ToLowerInvariant();
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(x => x.TypeOf).Must(type => SetupStateHelper.IsValidType(type)).WithMessage("TypeOf must be one of the allowed values.");
        RuleFor(m => m.StateType).NotEmpty();
        RuleFor(m => m.Name).NotEmpty().Length(1, 128);
        RuleFor(m => m.Sequence).NotNull();
        RuleFor(m => m.Description).Length(0, 256);
    }
}

public static class SetupStateHelper
{
    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    private static readonly List<string> allowedTypeOfValues = [
            nameof(Core.Project.ProjectInfo)
            ];

    public static List<string> AllowedTypeOfValues => allowedTypeOfValues;

    public static bool IsValidType(string type)
    {
        // Check if type is null or empty
        if (string.IsNullOrEmpty(type))
        {
            return false;
        }

        // Check if type is in any of the allowed value lists
        foreach (var list in SetupStateHelper.AllowedTypeOfValues)
        {
            if (list.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    public static async Task<List<SetupState>> GetProjectStatesAync(FeatureContext context, CancellationToken token)
        => await context.Set<SetupState>()
            .Where(q => q.TypeOf == nameof(Core.Project.ProjectInfo))
            .OrderBy(o => o.TypeOf).ThenBy(o => o.Sequence).ThenBy(o => o.Name)
            .ToListAsync(token);

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPSTATE_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateFound(string name) => Result.Fail(Messages.DUPLICATE_SETUPSTATE_NAME.Replace("{0}", name.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string typeOf, string name, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();

        return await context.Set<SetupState>()
            .AnyAsync(pr => pr.TypeOf == typeOf && pr.Name.ToLower() == name
                && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<SetupState, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> { }

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

public sealed record ListResult : EntityFlow.ListResult<ListModel> { }

internal sealed class ListHandler :
    EntityFlow.ListHandler<SetupState, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        IQueryable<SetupState> query = _dbcontext.Set<SetupState>();

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
        else query = query.OrderBy(o => o.TypeOf).ThenBy(o => o.Sequence).ThenBy(o => o.Name);

        int pageSize = EntityFlow.ListQuery.PageSize;
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

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

public sealed class CreateValidator : ModelValidator<CreateCommand> 
{
    public CreateValidator(FeatureContext dbContext) :
        base()
    {
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupStateHelper.IsDuplicateNameAsync(dbContext, rec.TypeOf, rec.Name);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPSTATE_NAME.Replace("{0}", x.Name.ToString()));
    }
}

internal sealed class CreateHandler : IRequestHandler<CreateCommand, Result<Ulid>>
{
    private readonly FeatureContext _dbcontext;

    public CreateHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<Result<Ulid>> Handle(CreateCommand message, CancellationToken token)
    {
        if (await SetupStateHelper.IsDuplicateNameAsync(_dbcontext, message.TypeOf, message.Name))
            return SetupStateHelper.DuplicateFound(message.Name);

        var record = new SetupState
        {
            Id = message.Id,
            TypeOf = message.TypeOf,
            StateType = message.StateType,
            Name = message.Name.Trim(),
            Description = message.Description?.Trim(),
            Sequence = message.Sequence,
            SearchIndex = message.SearchIndex,
            CreatedDT = DateTime.UtcNow
        };
        await _dbcontext.Set<SetupState>().AddAsync(record, token);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok(record.Id);
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
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupStateHelper.IsDuplicateNameAsync(dbContext, rec.TypeOf, rec.Name, rec.Id);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPSTATE_NAME.Replace("{0}", x.Name.ToString()));
    }
}

internal class UpdateCommandMapping : Profile 
{ 
    public UpdateCommandMapping() => CreateProjection<SetupState, UpdateCommand>(); 
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
        var command = await _dbcontext.Set<SetupState>()
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupStateHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<SetupState>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return SetupStateHelper.RecordNotFound(message.Id);

        if (await SetupStateHelper.IsDuplicateNameAsync(_dbcontext, message.TypeOf, message.Name, record.Id))
            return SetupStateHelper.DuplicateFound(message.Name);

        record.TypeOf = message.TypeOf;
        record.StateType = message.StateType;
        record.Name = message.Name.Trim();
        record.Description = message.Description?.Trim();
        record.Sequence = message.Sequence;
        record.SearchIndex = message.SearchIndex;
        record.ModifiedDT = DateTime.UtcNow;

        _dbcontext.Set<SetupState>().Update(record);
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
        CreateProjection<SetupState, DeleteCommand>();
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
        var command = await _dbcontext.Set<SetupState>()
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupStateHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<SetupState>().FindAsync([message.Id], cancellationToken: token);
        if(record is null)
            return SetupStateHelper.RecordNotFound(message.Id);

        _dbcontext.Set<SetupState>().Remove(record);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}

//
// IMPORT
//

public sealed record ImportModel : Model
{
    [NotMapped]
    [Display(Name = nameof(IsSelected), ResourceType = typeof(Localization))]
    public bool IsSelected { get; set; }
}

public sealed record ImportQuery : EntityFlow.ListQuery, IRequest<Result<ImportResult>>
{
}

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery> { public ImportQueryValidator() { } }

public sealed record ImportResult : EntityFlow.ListResult<ImportModel>
{
}

public sealed record ImportCommand : IRequest<Result>
{
    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand> { public ImportCommandValidator() { } }

internal sealed class ImportQueryHandler :
    EntityFlow.ListHandler<SetupState, ImportModel>,
    IRequestHandler<ImportQuery, Result<ImportResult>>
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImportQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, IWebHostEnvironment webHostEnvironment)
        : base(dbcontext, configuration)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Result<ImportResult>> Handle(ImportQuery message, CancellationToken token)
    {
        string wwwrootPath = _webHostEnvironment.WebRootPath;

        var filePath = Path.Combine(wwwrootPath, "data", "states.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The states.json file was not found.", filePath);
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        List<ImportModel>? records = await JsonSerializer.DeserializeAsync<List<ImportModel>>(stream, cancellationToken: token);
        records ??= [];

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            var searchFor = searchString.ToLowerInvariant();
            records = records.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
        }

        records = [.. records.OrderBy(o => o.TypeOf).ThenBy(o => o.Sequence).ThenBy(o => o.Name)];
        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;

        return new ImportResult()
        {
            CurrentFilter = searchString,
            SearchText = searchString,
            SortOrder = message.SortOrder,
            Results = new PaginatedList<ImportModel>(
                records.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                records.Count,
                pageNumber,
                pageSize)
        };
    }
}

internal class ImportCommandHandler : IRequestHandler<ImportCommand, Result>
{
    private readonly FeatureContext _dbcontext;

    public ImportCommandHandler(FeatureContext context)
    {
        _dbcontext = context;

        // TODO: Needed for the CreateTenantWorker to be able to overwrite blank TenantId values
        _dbcontext.TenantMismatchMode = Finbuckle.MultiTenant.EntityFrameworkCore.TenantMismatchMode.Overwrite;
    }

    public async Task<Result> Handle(ImportCommand message, CancellationToken token)
    {
        if (message is null || message.Items is null || message.Items.Count < 0)
            return Result.Ok();

        bool changes = false;
        foreach (var item in message.Items ?? [])
        {
            if (await SetupStateHelper.IsDuplicateNameAsync(_dbcontext, item.TypeOf, item.Name))
                continue;

            var record = new SetupState
            {
                Id = Ulid.NewUlid(),
                TypeOf = item.TypeOf,
                StateType = item.StateType,
                Name = item.Name.Trim(),
                Sequence = item.Sequence,
                Description = item.Description?.Trim(),
                SearchIndex = item.SearchIndex,
                CreatedDT = DateTime.UtcNow
            };

            await _dbcontext.Set<SetupState>().AddAsync(record, token);
            changes = true;
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}