using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Huybrechts.App.Features.Platform.PlatformDefaultUnitFlow;

public record Model
{
    public Ulid Id { get; init; }

    [Display(Name = "Platform", ResourceType = typeof(Localization))]
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Platform", ResourceType = typeof(Localization))]
    public string PlatformInfoName { get; set; } = string.Empty;

    [Display(Name = "SetupUnit", ResourceType = typeof(Localization))]
    public Ulid SetupUnitId { get; init; } = Ulid.Empty;

    [Display(Name = "SetupUnit", ResourceType = typeof(Localization))]
    public string SetupUnitName { get; set; } = string.Empty;

    [Display(Name = nameof(Sequence), ResourceType = typeof(Localization))]
    public int Sequence { get; set; } = 0;

    [Display(Name = nameof(ServiceName), ResourceType = typeof(Localization))]
    public string? ServiceName { get; set; }

    [Display(Name = nameof(ProductName), ResourceType = typeof(Localization))]
    public string? ProductName { get; set; }

    [Display(Name = nameof(SkuName), ResourceType = typeof(Localization))]
    public string? SkuName { get; set; }

    [Display(Name = nameof(MeterName), ResourceType = typeof(Localization))]
    public string? MeterName { get; set; }

    [Display(Name = nameof(UnitOfMeasure), ResourceType = typeof(Localization))]
    public string? UnitOfMeasure { get; set; }

    [Precision(12, 6)]
    [Display(Name = nameof(UnitFactor), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:N6}", ApplyFormatInEditMode = true)]
    public decimal UnitFactor { get; set; } = 0;

    [Precision(12, 4)]
    [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
    [Display(Name = nameof(DefaultValue), ResourceType = typeof(Localization))]
    public decimal DefaultValue { get; set; } = 0;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(IsDefaultPlatformRateUnit), ResourceType = typeof(Localization))]
    public bool IsDefaultPlatformRateUnit { get; set; } = false;

    [Display(Name = nameof(IsDefaultProjectComponentUnit), ResourceType = typeof(Localization))]
    public bool IsDefaultProjectComponentUnit { get; set; } = false;

    [Display(Name = nameof(Variable), ResourceType = typeof(Localization))]
    public string? Variable { get; set; }

    [Display(Name = nameof(Expression), ResourceType = typeof(Localization))]
    public string? Expression { get; set; }

    public string SearchIndex => PlatformDefaultUnitHelper.GetSearchIndex(ServiceName, ProductName, SkuName, MeterName, UnitOfMeasure, SetupUnitName, Description, Variable);
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.Description).NotEmpty().Length(1, 256);

        RuleFor(m => m.Sequence).NotNull().LessThan(0).LessThan(int.MaxValue);
        RuleFor(m => m.ServiceName).Length(0, 128);
        RuleFor(m => m.ProductName).Length(0, 128);
        RuleFor(m => m.SkuName).Length(0, 128);
        RuleFor(m => m.MeterName).Length(0, 128);
        RuleFor(m => m.UnitOfMeasure).Length(0, 64);
        RuleFor(m => m.UnitFactor).NotNull();
        RuleFor(m => m.DefaultValue).NotNull();
        RuleFor(m => m.Variable).Length(0, 128);
        RuleFor(m => m.Expression).Length(0, 256);
    }
}

public static class PlatformDefaultUnitHelper
{
    public static CreateCommand CreateNew(
        PlatformInfo platform
        ) => new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platform.Id,
            PlatformInfoName = platform.Name,
            PlatformInfo = platform
        };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<List<PlatformDefaultUnit>> GetDefaultUnitsFor(FeatureContext context, PlatformRate rate, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(rate);
        var units = await context.PlatformDefaultUnits
            .Where(unit => unit.PlatformInfoId == rate.PlatformInfoId &&
                           (rate.ServiceName == null || (unit.ServiceName != null && unit.ServiceName.ToLower() == rate.ServiceName.ToLower())) &&
                           (rate.ProductName == null || (unit.ProductName != null && unit.ProductName.ToLower() == rate.ProductName.ToLower())) &&
                           (rate.SkuName == null || (unit.SkuName != null && unit.SkuName.ToLower() == rate.SkuName.ToLower())) &&
                           (rate.MeterName == null || (unit.MeterName != null && unit.MeterName.ToLower() == rate.MeterName.ToLower())) &&
                           unit.IsDefaultPlatformRateUnit)
            .OrderBy(unit => unit.ServiceName)
            .ThenBy(unit => unit.ProductName)
            .ThenBy(unit => unit.SkuName)
            .ThenBy(unit => unit.Sequence)
            .ThenBy(unit => unit.MeterName ?? string.Empty)
            .ToListAsync(token);
        return units;
    }

    public static string GetSearchIndex(
        string? service, string? product, string? sku, string? meter, string? unitOfMeasure, string? setupUnit, string? description, string? variable)
        => $"{unitOfMeasure}~{setupUnit}~{description}~{service}~{product}~{sku}~{meter}~{variable}".ToLowerInvariant();

    internal static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    internal static Result UnitNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPUNIT_ID.Replace("{0}", id.ToString()));

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMDEFAULTUNIT_ID.Replace("{0}", id.ToString()));
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<PlatformDefaultUnit, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> { public Ulid? PlatformInfoId { get; set; } = Ulid.Empty; }

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { RuleFor(x => x.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty); } }

public sealed record ListResult : EntityFlow.ListResult<ListModel>
{
    public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

    public PlatformInfo PlatformInfo { get; set; } = new();
}

internal sealed class ListHandler :
    EntityFlow.ListHandler<PlatformRate, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == message.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId ?? Ulid.Empty);

        IQueryable<PlatformDefaultUnit> query = _dbcontext.Set<PlatformDefaultUnit>();

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            string searchFor = searchString.ToLowerInvariant();
            query = query.Where(q =>
                q.PlatformInfoId == message.PlatformInfoId
                && (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)));
        }
        else
        {
            query = query.Where(q => q.PlatformInfoId == message.PlatformInfoId);
        }

        if (!string.IsNullOrEmpty(message.SortOrder))
        {
            query = query.OrderBy(message.SortOrder);
        }
        else query = query
            .OrderBy(o => o.ServiceName)
            .ThenBy(o => o.ProductName)
            .ThenBy(o => o.SkuName)
            .ThenBy(o => o.Sequence)
            .ThenBy(o => o.MeterName);

        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await query
            .Include(i => i.SetupUnit)
            .ProjectTo<ListModel>(_configuration)
            .PaginatedListAsync(pageNumber, pageSize);

        var model = new ListResult
        {
            PlatformInfoId = message.PlatformInfoId,
            PlatformInfo = platform,
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

public sealed record CreateQuery : IRequest<Result<CreateCommand>> { public Ulid PlatformInfoId { get; set; } = Ulid.Empty; }

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery> { public CreateQueryValidator() { RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty); } }

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
{
    public PlatformInfo PlatformInfo { get; set; } = new();

    public List<SetupUnit> SetupUnits { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
{
    public CreateCommandValidator(FeatureContext dbContext) : base()
    {
        RuleFor(x => x.PlatformInfoId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<PlatformInfo>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PLATFORM_ID.Replace("{0}", m.PlatformInfoId.ToString()));
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
        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == message.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        var record = PlatformDefaultUnitHelper.CreateNew(platform);
        record.SetupUnits = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token);

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
        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        var unit = await _dbcontext.Set<SetupUnit>().FindAsync([message.SetupUnitId], cancellationToken: token);
        //if (unit is null)
        //    return PlatformDefaultUnitHelper.UnitNotFound(message.SetupUnitId);

        var record = new PlatformDefaultUnit
        {
            Id = message.Id,
            PlatformInfoId = message.PlatformInfoId,
            PlatformInfo = platform,
            SetupUnitId = unit?.Id,
            SetupUnit = unit is not null ? unit : null,
            Description = message.Description,
            SearchIndex = message.SearchIndex?.Trim(),
            CreatedDT = DateTime.UtcNow,
            IsDefaultPlatformRateUnit = message.IsDefaultPlatformRateUnit,
            IsDefaultProjectComponentUnit = message.IsDefaultProjectComponentUnit,
            UnitOfMeasure = message.UnitOfMeasure,
            Sequence = message.Sequence,
            ServiceName = message.ServiceName,
            ProductName = message.ProductName,
            SkuName = message.SkuName,
            MeterName = message.MeterName,
            Variable = message.Variable,
            Expression = message.Expression,
            UnitFactor = decimal.Round(message.UnitFactor, 6, MidpointRounding.ToEven),
            DefaultValue = decimal.Round(message.DefaultValue, 6, MidpointRounding.ToEven),
        };

        await _dbcontext.Set<PlatformDefaultUnit>().AddAsync(record, token);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok(record.Id);
    }
}

//
// UPDATE
//

public sealed record UpdateQuery : IRequest<Result<UpdateCommand>> { public Ulid Id { get; init; } }

public sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery> { public UpdateQueryValidator() { RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty); } }

public record UpdateCommand : Model, IRequest<Result> 
{
    public PlatformInfo PlatformInfo { get; set; } = new();

    public List<SetupUnit> SetupUnits { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.PlatformInfoId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<PlatformInfo>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PLATFORM_ID.Replace("{0}", m.PlatformInfoId.ToString()));
    }
}

internal class UpdateCommandMapping : Profile { public UpdateCommandMapping() =>  CreateProjection<PlatformDefaultUnit, UpdateCommand>(); }

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
        var record = await _dbcontext.Set<PlatformDefaultUnit>()
            .Include(i => i.SetupUnit)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null) 
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(record.PlatformInfoId);

        record.PlatformInfo = platform;
        record.SetupUnits = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token);

        return Result.Ok(record);
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
        var record = await _dbcontext.Set<PlatformDefaultUnit>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        var unit = await _dbcontext.Set<SetupUnit>().FindAsync([message.SetupUnitId], cancellationToken: token);
        //if (unit is null)
        //    return PlatformDefaultUnitHelper.UnitNotFound(message.SetupUnitId);

        record.SetupUnit = unit;
        if (unit is null)
            record.SetupUnitId = null;
        record.Description = message.Description?.Trim();
        record.SearchIndex = message.SearchIndex?.Trim();
        record.ModifiedDT = DateTime.UtcNow;
        
        record.UnitOfMeasure = message.UnitOfMeasure;
        record.UnitFactor = decimal.Round(message.UnitFactor, 6, MidpointRounding.ToEven);
        record.DefaultValue = decimal.Round(message.DefaultValue, 6, MidpointRounding.ToEven);
        record.IsDefaultPlatformRateUnit = message.IsDefaultPlatformRateUnit;
        record.IsDefaultProjectComponentUnit = message.IsDefaultProjectComponentUnit;
        record.UnitOfMeasure = message.UnitOfMeasure;
        record.Sequence = message.Sequence;
        record.ServiceName = message.ServiceName;
        record.ProductName = message.ProductName;
        record.SkuName = message.SkuName;
        record.MeterName = message.MeterName;
        record.Variable = message.Variable;
        record.Expression = message.Expression;

        _dbcontext.Set<PlatformDefaultUnit>().Update(record);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}

//
// DELETE
//

public sealed record DeleteQuery : IRequest<Result<DeleteCommand>> { public Ulid Id { get; init; } }

public class DeleteQueryValidator : AbstractValidator<DeleteQuery> { public DeleteQueryValidator() { RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty); } }

public sealed record DeleteCommand : Model, IRequest<Result>
{
    public PlatformInfo PlatformInfo { get; set; } = new();

    public List<SetupUnit> SetupUnits { get; set; } = [];
}

public sealed class DeleteCommandValidator : AbstractValidator<DeleteCommand> { public DeleteCommandValidator() { RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty); } }

internal sealed class DeleteCommandMapping : Profile { public DeleteCommandMapping() => CreateProjection<PlatformDefaultUnit, DeleteCommand>(); }

internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, Result<DeleteCommand>>
{
    private readonly FeatureContext _dbcontext;
    private readonly IConfigurationProvider _configuration;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public DeleteQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _dbcontext = dbcontext;
        _configuration = configuration;
        _cache = cache;
    }

    public async Task<Result<DeleteCommand>> Handle(DeleteQuery message, CancellationToken token)
    {
        var record = await _dbcontext.Set<PlatformDefaultUnit>()
            .Include(i => i.SetupUnit)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null)
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(record.PlatformInfoId);

        record.PlatformInfo = platform;
        record.SetupUnits = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token);

        return Result.Ok(record);
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
        var record = await _dbcontext.Set<PlatformDefaultUnit>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        _dbcontext.Set<PlatformDefaultUnit>().Remove(record);
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

public sealed record ImportQuery : EntityFlow.ListQuery, IRequest<Result<ImportResult>> { public Ulid PlatformInfoId { get; set; } = Ulid.Empty; }

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery> { public ImportQueryValidator() { RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty); } }

public sealed record ImportResult : EntityFlow.ListResult<ImportModel> 
{
    public PlatformInfo PlatformInfo { get; set; } = new();

    public List<SetupUnit> SetupUnits { get; set; } = [];
}

public sealed record ImportCommand : IRequest<Result> 
{ 
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty; 

    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand> { public ImportCommandValidator() { } }

internal sealed class ImportQueryHandler :
    EntityFlow.ListHandler<PlatformDefaultUnit, ImportModel>,
    IRequestHandler<ImportQuery, Result<ImportResult>>
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public ImportQueryHandler(
        FeatureContext dbcontext, 
        IConfigurationProvider configuration, 
        IWebHostEnvironment webHostEnvironment, 
        Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        : base(dbcontext, configuration)
    {
        _webHostEnvironment = webHostEnvironment;
        _cache = cache;
    }

    public async Task<Result<ImportResult>> Handle(ImportQuery message, CancellationToken token)
    {
        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        string wwwrootPath = _webHostEnvironment.WebRootPath;
        var fileName = "defaultunits_" + platform.Provider.ToString().ToLower() + ".json";
        var filePath = Path.Combine(wwwrootPath, "data", fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The {fileName} file was not found.", filePath);
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        List<ImportModel>? records = await JsonSerializer.DeserializeAsync<List<ImportModel>>(stream, cancellationToken: token);
        records ??= [];
        records.ForEach(record => { record.PlatformInfoId = platform.Id; });

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            var searchFor = searchString.ToLowerInvariant();
            records = records.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
        }

        records = [.. records
            .OrderBy(o => o.ServiceName)
            .ThenBy(o => o.ProductName)
            .ThenBy(o => o.SkuName)
            .ThenBy(o => o.Sequence)
            .ThenBy(o => o.MeterName)
        ];
        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;

        return new ImportResult()
        {
            PlatformInfo = platform,
            SetupUnits = new SetupUnitHelper(_cache, _dbcontext).GetSetupUnitsAsync(token: token),
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

    public ImportCommandHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public async Task<Result> Handle(ImportCommand message, CancellationToken token)
    {
        if (message is null || message.Items is null || message.Items.Count < 0)
            return Result.Ok();

        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        bool changes = false;
        foreach (var item in message.Items)
        {
            var setupunit = await _dbcontext.Set<SetupUnit>().FirstOrDefaultAsync(q =>
                (q.Code.ToUpper() == item.SetupUnitName)
                || (q.Name.ToLower() == item.SetupUnitName.ToLower()),
                cancellationToken: token);

            PlatformDefaultUnit record = new()
            {
                Id = Ulid.NewUlid(),
                PlatformInfo = platform,
                PlatformInfoId = item.PlatformInfoId,
                SetupUnit = setupunit,
                SetupUnitId = setupunit?.Id,
                Description = item.Description?.Trim(),
                CreatedDT = DateTime.UtcNow,
                DefaultValue = item.DefaultValue,
                UnitFactor = item.UnitFactor,
                UnitOfMeasure = item.UnitOfMeasure,
                IsDefaultPlatformRateUnit = item.IsDefaultPlatformRateUnit,
                IsDefaultProjectComponentUnit = item.IsDefaultProjectComponentUnit,
                Sequence = item.Sequence,
                ServiceName = item.ServiceName,
                ProductName = item.ProductName,
                SkuName = item.SkuName,
                MeterName = item.MeterName,
                Variable = item.Variable,
                Expression = item.Expression,
                SearchIndex = PlatformDefaultUnitHelper.GetSearchIndex(item.ServiceName, item.ProductName, item.SkuName, item.MeterName, item.UnitOfMeasure, item.SetupUnitName, item.Description, item.Variable)
            };

            await _dbcontext.Set<PlatformDefaultUnit>().AddAsync(record, token);
            changes = true;
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}
