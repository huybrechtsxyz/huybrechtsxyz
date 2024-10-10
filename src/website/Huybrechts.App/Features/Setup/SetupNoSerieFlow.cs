using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Huybrechts.App.Features.Setup.SetupNoSerieFlow;

public record Model
{
    public Ulid Id { get; init; }

    [Display(Name = nameof(TypeOf), ResourceType = typeof(Localization))]
    public string TypeOf { get; set; } = string.Empty;

    [Display(Name = nameof(TypeCode), ResourceType = typeof(Localization))]
    public string TypeCode { get; set; } = string.Empty;

    [Display(Name = nameof(TypeValue), ResourceType = typeof(Localization))]
    public string TypeValue { get; set; } = string.Empty;

    [Display(Name = nameof(Format), ResourceType = typeof(Localization))]
    public string Format { get; set; } = string.Empty;

    [Display(Name = nameof(Counter), ResourceType = typeof(Localization))]
    public int Counter { get; set; } = 0;

    [Display(Name = nameof(Maximum), ResourceType = typeof(Localization))]
    public int Maximum { get; set; } = 999999999;

    [Display(Name = nameof(IsDisabled), ResourceType = typeof(Localization))]
    public bool IsDisabled { get; set; } = false;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    public string SearchIndex => $"{TypeOf}~{TypeCode}~{TypeValue}~{Format}~{Description}".ToLowerInvariant();
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(x => x.TypeOf).NotEmpty().Length(1, 64);
        RuleFor(x => x.TypeOf).Must(type => SetupNoSerieHelper.IsValidType(type)).WithMessage("TypeOf must be one of the allowed values.");
        RuleFor(m => m.TypeCode).NotEmpty().Length(1, 64);
        RuleFor(m => m.TypeValue).NotNull().Length(0, 64);
        RuleFor(m => m.Format).NotEmpty().Length(1, 128);
        RuleFor(m => m.Counter).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(m => m.Maximum).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999999);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

public static class SetupNoSerieHelper
{
    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    public static void CopyFields(Model model, SetupNoSerie entity)
    {
        entity.TypeOf = model.TypeOf.Trim();
        entity.TypeCode = model.TypeCode.Trim();
        entity.TypeValue = model.TypeValue.Trim();
        entity.Format = model.Format.Trim();
        entity.Counter = model.Counter;
        entity.Maximum = model.Maximum;
        entity.IsDisabled = model.IsDisabled;
        entity.Description = model.Description?.Trim();
        entity.SearchIndex = model.SearchIndex;
    }

    private static readonly List<string> allowedTypeOfValues = [
            "ProjectCode"
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

    internal static Result DuplicateFound(string cat, string subcat) => Result.Fail(Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", cat.ToString()).Replace("{1}", subcat.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateTypeAsync(DbContext context, string typeOf, string typeCode, string typeValue, Ulid? currentId = null)
    {
        typeCode = typeCode.ToLower().Trim();
        typeValue = typeValue.ToLower().Trim();

        return await context.Set<SetupNoSerie>()
            .AnyAsync(pr => pr.TypeOf == typeOf && pr.TypeCode.ToLower() == typeCode && pr.TypeValue.ToLower() == typeValue.ToLower().Trim()
                && (!currentId.HasValue || pr.Id != currentId.Value));
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
        else query = query.OrderBy(o => o.TypeOf).ThenBy(o => o.TypeCode).ThenBy(o => o.TypeValue);

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
            bool exists = await SetupNoSerieHelper.IsDuplicateTypeAsync(dbContext, rec.TypeOf, rec.TypeCode, rec.TypeValue);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", $"{x.TypeOf} - {x.TypeCode} - {x.TypeValue}"));
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
        if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, message.TypeOf, message.TypeCode, message.TypeValue))
            return SetupNoSerieHelper.DuplicateFound(message.TypeCode, message.TypeValue);

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

public record UpdateCommand : Model, IRequest<Result> { }

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupNoSerieHelper.IsDuplicateTypeAsync(dbContext, rec.TypeOf, rec.TypeCode, rec.TypeValue, rec.Id);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPNOSERIE.Replace("{0}", $"{x.TypeOf} - {x.TypeCode} - {x.TypeValue}"));
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

        if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, message.TypeOf, message.TypeCode, message.TypeValue, record.Id))
            return SetupNoSerieHelper.DuplicateFound(message.TypeCode, message.TypeValue);

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

        records = [.. records.OrderBy(o => o.TypeOf).ThenBy(o => o.TypeCode).ThenBy(o => o.TypeValue)];
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
            if (await SetupNoSerieHelper.IsDuplicateTypeAsync(_dbcontext, item.TypeOf, item.TypeCode, item.TypeValue))
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