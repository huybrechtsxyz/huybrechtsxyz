using AutoMapper;
using AutoMapper.QueryableExtensions;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupTypeFlow;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Reflection.Emit;
using System.Text.Json;

namespace Huybrechts.App.Features.Setup.SetupNoSerieFlow;

public record Model
{
    public Ulid Id { get; init; }

    [Display(Name = nameof(TypeOf), ResourceType = typeof(Localization))]
    public string TypeOf { get; set; } = string.Empty;

    [Display(Name = nameof(TypeValue), ResourceType = typeof(Localization))]
    public string TypeValue { get; set; } = string.Empty;

    [Display(Name = nameof(Format), ResourceType = typeof(Localization))]
    public string Format { get; set; } = string.Empty;

    [Range(0,999999999)]
    [Display(Name = nameof(StartCounter), ResourceType = typeof(Localization))]
    public int StartCounter { get; set; } = 1;

    [Range(0, 999999999)]
    [Display(Name = nameof(Increment), ResourceType = typeof(Localization))]
    public int Increment { get; set; } = 0;

    [Range(0, 999999999)]
    [Display(Name = nameof(Maximum), ResourceType = typeof(Localization))]
    public int Maximum { get; set; } = 999999999;

    [Display(Name = nameof(AutomaticReset), ResourceType = typeof(Localization))]
    public bool AutomaticReset { get; set; } = false;

    [Display(Name = nameof(LastCounter), ResourceType = typeof(Localization))]
    public int LastCounter { get; set; } = 0;

    [Display(Name = nameof(LastPrefix), ResourceType = typeof(Localization))]
    public string LastPrefix { get; set; } = string.Empty;

    [Display(Name = nameof(LastValue), ResourceType = typeof(Localization))]
    public string LastValue { get; set; } = string.Empty;

    [Display(Name = nameof(IsDisabled), ResourceType = typeof(Localization))]
    public bool IsDisabled { get; set; } = false;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    public string SearchIndex => $"{TypeOf}~{TypeValue}~{Format}~{Description}".ToLowerInvariant();
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(x => x.TypeOf).NotEmpty().Length(1, 64);
        RuleFor(x => x.TypeOf).Must(type => SetupNoSerieHelper.IsValidType(type)).WithMessage("TypeOf must be one of the allowed values.");
        RuleFor(m => m.TypeValue).NotNull().Length(0, 64);
        RuleFor(m => m.Format).NotEmpty().Length(1, 128);
        RuleFor(m => m.StartCounter).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(m => m.Increment).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(m => m.Maximum).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(m => m.LastCounter).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(m => m.LastPrefix).NotNull().Length(0, 64);
        RuleFor(m => m.LastValue).NotNull().Length(0, 64);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

public static class SetupNoSerieHelper
{
    public static async Task<Result<string>> GenerateAsync(
        FeatureContext context, 
        NoSerieQuery message,
        IValidator<NoSerieQuery> validator,
        CancellationToken token)
    {
        // Get all no. series for that object type
        message.TypeValue = message.TypeValue.ToUpper().Trim();
        var list = await context.Set<SetupNoSerie>()
            .Where(q => q.TypeOf == message.TypeOf)
            .ToListAsync(token);

        // Get the next number
        NumberSeriesGenerator generator = new(validator);
        Result<SetupNoSerie> result = generator.Generate(message, list);
        if (result.IsFailed)
            return result.ToResult<string>();
        if (message.DoPeek)
            return Result.Ok(result.Value.LastValue);

        // Update the number
        SetupNoSerie entity = result.Value;
        var record = await context.Set<SetupNoSerie>().FindAsync([entity.Id], cancellationToken: token);
        if (record is null)
            return RecordNotFound(entity.Id);
        record.ModifiedDT = DateTime.UtcNow;
        record.LastCounter = entity.LastCounter;
        record.LastPrefix = entity.LastPrefix;
        record.LastValue = entity.LastValue;

        context.Set<SetupNoSerie>().Update(record);
        return Result.Ok(record.LastValue);
    }

    public const string PROJECTCODE = "ProjectCode";

    private static readonly List<string> allowedTypeOfValues = [
            PROJECTCODE
            ];

    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    public static void CopyFields(Model model, SetupNoSerie entity)
    {
        entity.TypeOf = model.TypeOf.Trim();
        entity.TypeValue = model.TypeValue.ToUpper().Trim();
        entity.Format = model.Format.Trim();
        entity.StartCounter = model.StartCounter;
        entity.Increment = model.Increment; 
        entity.Maximum = model.Maximum;
        entity.AutomaticReset = model.AutomaticReset;
        entity.LastCounter = model.LastCounter;
        entity.LastPrefix = model.LastPrefix.Trim();
        entity.LastValue = model.LastValue.Trim();
        entity.IsDisabled = model.IsDisabled;
        entity.Description = model.Description?.Trim();
        entity.SearchIndex = model.SearchIndex;
    }

    public static List<string> AllowedTypeOfValues => allowedTypeOfValues;

    public static bool IsValidType(string type)
    {
        // Check if type is null or empty
        if (string.IsNullOrEmpty(type))
        {
            return false;
        }

        // Check if type is in any of the allowed value lists
        foreach (var list in SetupNoSerieHelper.AllowedTypeOfValues)
        {
            if (list.Contains(type))
            {
                return true;
            }
        }

        return false;
    }

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPNOSERIE_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateFound(string value) => Result.Fail(Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", value));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateTypeAsync(DbContext context, string typeOf, string typeValue, Ulid? currentId = null)
    {
        typeValue = typeValue.ToUpper().Trim();

        return await context.Set<SetupNoSerie>()
            .AnyAsync(pr => pr.TypeOf == typeOf && pr.TypeValue.ToUpper() == typeValue.ToUpper().Trim()
                && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

//
// GET NEW NUMBER
//

public sealed record NoSerieQuery : IRequest<Result<string>>
{
    public string TypeOf { get; set; } = string.Empty;

    public string TypeValue { get; set; } = string.Empty;

    public DateTime DateTime { get; set; } = DateTime.Now;

    public bool DoPeek { get; set; } = true;
}

public sealed class NoSerieQueryValidator : AbstractValidator<NoSerieQuery>
{
    public NoSerieQueryValidator() : base()
    {
        RuleFor(x => x.TypeOf).NotEmpty().Length(1, 64);
        RuleFor(x => x.TypeOf).Must(type => SetupNoSerieHelper.IsValidType(type)).WithMessage("TypeOf must be one of the allowed values.");
        RuleFor(x => x.TypeValue).NotEmpty();
    }
}

internal sealed class NoSerieHandler : IRequestHandler<NoSerieQuery, Result<string>>
{
    private readonly FeatureContext _dbcontext;
    private readonly NumberSeriesGenerator _generator;
    private readonly IValidator<NoSerieQuery> _validator;

    public NoSerieHandler(FeatureContext dbcontext, IValidator<NoSerieQuery> validator)
    {
        _dbcontext = dbcontext;
        _validator = validator;
        _generator = new(validator);
    }

    public async Task<Result<string>> Handle(NoSerieQuery message, CancellationToken token)
    {
        var result = await SetupNoSerieHelper.GenerateAsync(_dbcontext, message, _validator, token);
        if (result.IsSuccess && !message.DoPeek)
            await _dbcontext.SaveChangesAsync(token);
        return result;
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<SetupNoSerie, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> { }

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

public sealed record ListResult : EntityFlow.ListResult<ListModel> { }

internal sealed class ListHandler :
    EntityFlow.ListHandler<SetupNoSerie, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        IQueryable<SetupNoSerie> query = _dbcontext.Set<SetupNoSerie>();

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
        else query = query.OrderBy(o => o.TypeOf).ThenBy(o => o.TypeValue);

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

public sealed record CreateQuery : IRequest<Result<CreateCommand>>
{
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery> { public CreateQueryValidator() { } }

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
{
    public List<SetupType> SetupTypes { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand> 
{
    public CreateCommandValidator(FeatureContext dbContext) :
        base()
    {
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupNoSerieHelper.IsDuplicateTypeAsync(dbContext, rec.TypeOf, rec.TypeValue);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", $"{x.TypeOf} - {x.TypeValue}"));
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
        var record = SetupNoSerieHelper.CreateNew();
        record.SetupTypes = await SetupTypeHelper.GetAllTypesAync(_dbcontext, token);
        return Result.Ok(record);
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
        if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, message.TypeOf, message.TypeValue))
            return SetupNoSerieHelper.DuplicateFound(message.TypeValue);

        var record = new SetupNoSerie
        {
            Id = message.Id,
            CreatedDT = DateTime.UtcNow
        };
        SetupNoSerieHelper.CopyFields(message, record);
        await _dbcontext.Set<SetupNoSerie>().AddAsync(record, token);
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

public record UpdateCommand : Model, IRequest<Result>
{
    public List<SetupType> SetupTypes { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupNoSerieHelper.IsDuplicateTypeAsync(dbContext, rec.TypeOf, rec.TypeValue, rec.Id);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", $"{x.TypeOf} - {x.TypeValue}"));
    }
}

internal class UpdateCommandMapping : Profile 
{ 
    public UpdateCommandMapping() => CreateProjection<SetupNoSerie, UpdateCommand>(); 
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
        var command = await _dbcontext.Set<SetupNoSerie>()
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupNoSerieHelper.RecordNotFound(message.Id ?? Ulid.Empty);

        command.SetupTypes = await SetupTypeHelper.GetAllTypesAync(_dbcontext, token);

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
        var record = await _dbcontext.Set<SetupNoSerie>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return SetupNoSerieHelper.RecordNotFound(message.Id);

        if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, message.TypeOf, message.TypeValue, record.Id))
            return SetupNoSerieHelper.DuplicateFound(message.TypeValue);

        record.ModifiedDT = DateTime.UtcNow;
        SetupNoSerieHelper.CopyFields(message, record);

        _dbcontext.Set<SetupNoSerie>().Update(record);
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
        CreateProjection<SetupNoSerie, DeleteCommand>();
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
        var command = await _dbcontext.Set<SetupNoSerie>()
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupNoSerieHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<SetupNoSerie>().FindAsync([message.Id], cancellationToken: token);
        if(record is null)
            return SetupNoSerieHelper.RecordNotFound(message.Id);

        _dbcontext.Set<SetupNoSerie>().Remove(record);
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

public sealed record ImportQuery : EntityFlow.ListQuery, IRequest<Result<ImportResult>> { }

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery> { public ImportQueryValidator() { } }

public sealed record ImportResult : EntityFlow.ListResult<ImportModel> { }

public sealed record ImportCommand : IRequest<Result>
{
    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand> { public ImportCommandValidator() { } }

internal sealed class ImportQueryHandler :
    EntityFlow.ListHandler<SetupNoSerie, ImportModel>,
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

        var filePath = Path.Combine(wwwrootPath, "data", "numberseries.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The numberseries.json file was not found.", filePath);
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

        records = [.. records.OrderBy(o => o.TypeOf).ThenBy(o => o.TypeValue)];
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
            if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, item.TypeOf, item.TypeValue))
                continue;

            var record = new SetupNoSerie
            {
                Id = Ulid.NewUlid(),
                CreatedDT = DateTime.UtcNow
            };
            SetupNoSerieHelper.CopyFields(item, record);

            await _dbcontext.Set<SetupNoSerie>().AddAsync(record, token);
            changes = true;
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}