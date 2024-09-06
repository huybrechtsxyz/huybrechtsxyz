using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Setup.SetupUnitFlow;
using Huybrechts.App.Services;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Platform.PlatformDefaultUnitFlow;

public static class PlatformDefaultUnitHelper
{
    public static CreateCommand CreateNew(
        PlatformInfo platform
        ) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = platform.Id,
        PlatformInfoName = platform.Name,
        Platform = platform
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<List<PlatformDefaultUnit>> GetDefaultUnitsFor(FeatureContext context, PlatformRate rate, bool save, CancellationToken token)
    {
        string unitOfMeasure = rate.UnitOfMeasure.ToLower().Trim();
        List<PlatformDefaultUnit> defaultUnits = await context.Set<PlatformDefaultUnit>()
            .Include(i => i.SetupUnit)
            .Where(q => q.PlatformInfoId == rate.PlatformInfoId && q.UnitOfMeasure.ToLower() == unitOfMeasure)
            .OrderBy(o => o.SetupUnit.Name)
            .ToListAsync(cancellationToken: token);

        if (defaultUnits is not null && defaultUnits.Count > 0)
            return defaultUnits;

        var setupUnit = await SetuptUnitHelper.FindOrCreateDefaultSetupUnitAsync(context, save, token);
        defaultUnits = [];
        defaultUnits.Add(new PlatformDefaultUnit()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = rate.PlatformInfoId,
            SetupUnit = setupUnit,
            SetupUnitId = setupUnit.Id,
            UnitOfMeasure = rate.UnitOfMeasure,
            UnitFactor = 1,
            DefaultValue = 1,
            Description = rate.UnitOfMeasure,
            SearchIndex = GetSearchIndex(rate.UnitOfMeasure, setupUnit.Name, rate.UnitOfMeasure),
            CreatedDT = DateTime.UtcNow,
        });

        return defaultUnits;
    }

    public static async Task<List<SetupUnit>> GetSetupUnitsAsync(FeatureContext dbcontext, CancellationToken token)
    {
        return await dbcontext.Set<SetupUnit>()
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken: token);
    }

    public static string GetSearchIndex(string unitOfMeasure, string setupUnit, string description)
        => $"{unitOfMeasure}~{setupUnit}~{description}".ToLowerInvariant();

    internal static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    internal static Result UnitNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPUNIT_ID.Replace("{0}", id.ToString()));

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMDEFAULTUNIT_ID.Replace("{0}", id.ToString()));
}

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
    public SetupUnit SetupUnit { get; init; } = new();

    [Display(Name = nameof(UnitOfMeasure), ResourceType = typeof(Localization))]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Precision(12, 6)]
    [Display(Name = nameof(UnitFactor), ResourceType = typeof(Localization))]
    public decimal UnitFactor { get; set; } = 0;

    [Precision(12, 4)]
    [Display(Name = nameof(DefaultValue), ResourceType = typeof(Localization))]
    public decimal DefaultValue { get; set; } = 0;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string Description { get; set; } = string.Empty;

    public string SearchIndex => PlatformDefaultUnitHelper.GetSearchIndex(UnitOfMeasure, SetupUnit.Name, Description);
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.Description).NotEmpty().Length(1, 128);

        RuleFor(m => m.UnitOfMeasure).NotNull();
        RuleFor(m => m.UnitFactor).NotNull();
        RuleFor(m => m.DefaultValue).NotNull();
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<PlatformDefaultUnit, ListModel>();
}

public sealed record ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>>
{
    public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult : EntityListFlow.Result<ListModel>
{
    public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

    public PlatformInfo Platform { get; set; } = new();
}

internal sealed class ListHandler :
    EntityListFlow.Handler<PlatformRate, ListModel>,
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
            .OrderBy(o => o.UnitOfMeasure)
            .ThenBy(o => o.SetupUnit.Name);

        int pageSize = EntityListFlow.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await query
            .Include(i => i.SetupUnit)
            .ProjectTo<ListModel>(_configuration)
            .PaginatedListAsync(pageNumber, pageSize);

        var model = new ListResult
        {
            PlatformInfoId = message.PlatformInfoId,
            Platform = platform,
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
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
{
    public PlatformInfo Platform { get; set; } = new();

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

    public CreateQueryHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<Result<CreateCommand>> Handle(CreateQuery message, CancellationToken token)
    {
        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == message.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        var record = PlatformDefaultUnitHelper.CreateNew(platform);
        record.SetupUnits = await PlatformDefaultUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        if (unit is null)
            return PlatformDefaultUnitHelper.UnitNotFound(message.SetupUnitId);

        var record = new PlatformDefaultUnit
        {
            Id = message.Id,
            PlatformInfoId = message.PlatformInfoId,
            PlatformInfo = platform,
            SetupUnitId = unit.Id,
            SetupUnit = unit,
            Description = message.Description,
            SearchIndex = message.SearchIndex?.Trim(),
            CreatedDT = DateTime.UtcNow,
            UnitOfMeasure = message.UnitOfMeasure,
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

public sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
{
    public UpdateQueryValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public record UpdateCommand : Model, IRequest<Result> 
{
    public PlatformInfo Platform { get; set; } = new();

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

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<PlatformDefaultUnit, UpdateCommand>();
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
        var record = await _dbcontext.Set<PlatformDefaultUnit>()
            .Include(i => i.SetupUnit)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null) 
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(record.PlatformInfoId);

        record.Platform = platform;
        record.SetupUnits = await PlatformDefaultUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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
        if (unit is null)
            return PlatformDefaultUnitHelper.UnitNotFound(message.SetupUnitId);

        record.Description = message.Description.Trim();
        record.SearchIndex = message.SearchIndex?.Trim();
        record.ModifiedDT = DateTime.UtcNow;

        record.SetupUnit = unit;
        record.UnitOfMeasure = message.UnitOfMeasure;
        record.UnitFactor = decimal.Round(message.UnitFactor, 6, MidpointRounding.ToEven);
        record.DefaultValue = decimal.Round(message.DefaultValue, 6, MidpointRounding.ToEven);

        _dbcontext.Set<PlatformDefaultUnit>().Update(record);
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
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record DeleteCommand : Model, IRequest<Result>
{
    public PlatformInfo Platform { get; set; } = new();

    public List<SetupUnit> SetupUnits { get; set; } = [];
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
        CreateProjection<PlatformDefaultUnit, DeleteCommand>();
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
        var record = await _dbcontext.Set<PlatformDefaultUnit>()
            .Include(i => i.SetupUnit)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null)
            return PlatformDefaultUnitHelper.RecordNotFound(message.Id);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformDefaultUnitHelper.PlatformNotFound(record.PlatformInfoId);

        record.Platform = platform;
        record.SetupUnits = await PlatformDefaultUnitHelper.GetSetupUnitsAsync(_dbcontext, token);

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

public sealed record ImportQuery : EntityListFlow.Query, IRequest<Result<ImportResult>>
{
    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;
}

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery>
{
    public ImportQueryValidator()
    {
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record ImportResult : EntityListFlow.Result<ImportModel>
{
    public PlatformInfo Platform { get; set; } = new();
}

public sealed record ImportCommand : IRequest<Result>
{
    public Ulid PlatformInfoId { get; set; }

    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand>
{
    public ImportCommandValidator()
    {
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

internal sealed class ImportQueryHandler :
    EntityListFlow.Handler<PlatformDefaultUnit, ImportModel>,
    IRequestHandler<ImportQuery, Result<ImportResult>>
{
    private readonly PlatformImportOptions _options;

    public ImportQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options)
        : base(dbcontext, configuration)
    {
        _options = options;
    }

    public async Task<Result<ImportResult>> Handle(ImportQuery message, CancellationToken token)
    {
        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        var searchString = message.SearchText ?? message.CurrentFilter;
        List<ImportModel> records = await GetAzureUnitsAsync(platform.Provider.ToString(), message.PlatformInfoId, searchString);

        if (!string.IsNullOrEmpty(searchString))
        {
            var searchFor = searchString.ToLowerInvariant();
            records = records.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
        }

        records = [.. records.OrderBy(o => o.UnitOfMeasure)];
        int pageSize = EntityListFlow.PageSize;
        int pageNumber = message.Page ?? 1;

        return new ImportResult()
        {
            Platform = platform,
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

    private async Task<List<ImportModel>> GetAzureUnitsAsync(string platform, Ulid platformInfoId, string searchString)
    {
        List<ImportModel> result = [];

        var service = new AzurePricingService(_options);
        var pricing = await service.GetUnitsAsync(searchString);

        if (pricing is null)
            return [];

        foreach (var item in pricing.Items ?? [])
        {
            if (item is null || string.IsNullOrEmpty(item.UnitOfMeasure))
                continue;

            result.Add(new ImportModel()
            {
                Id = Ulid.NewUlid(),
                PlatformInfoId = platformInfoId,
                UnitOfMeasure = item.UnitOfMeasure,
                Description = item.UnitOfMeasure,
                DefaultValue = 1,
                UnitFactor = 1
            });
        }

        return result;
    }
}

internal class ImportCommandHandler : IRequestHandler<ImportCommand, Result>
{
    private readonly FeatureContext _dbcontext;

    public ImportCommandHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<Result> Handle(ImportCommand message, CancellationToken token)
    {
        if (message is null || message.Items is null || message.Items.Count < 0)
            return Result.Ok();

        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformDefaultUnitHelper.PlatformNotFound(message.PlatformInfoId);

        var setupUnit = await SetuptUnitHelper.FindOrCreateDefaultSetupUnitAsync(_dbcontext, false, token);

        bool changes = false;
        foreach (var item in message.Items)
        {
            PlatformDefaultUnit record = new()
            {
                Id = Ulid.NewUlid(),
                PlatformInfo = platform,
                PlatformInfoId = item.PlatformInfoId,
                Description = item.Description?.Trim(),
                CreatedDT = DateTime.UtcNow,
                //SetupUnit = setupUnit,
                SetupUnitId = setupUnit.Id,
                DefaultValue = item.DefaultValue,
                UnitFactor = item.UnitFactor,
                UnitOfMeasure = item.UnitOfMeasure,
                SearchIndex = PlatformDefaultUnitHelper.GetSearchIndex(item.UnitOfMeasure, item.SetupUnit.Name, item.UnitOfMeasure)
            };

            await _dbcontext.Set<PlatformDefaultUnit>().AddAsync(record, token);
            changes = true;
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}
