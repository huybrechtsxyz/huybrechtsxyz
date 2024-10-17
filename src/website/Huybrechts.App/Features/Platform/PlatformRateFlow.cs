using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Services;
using Huybrechts.Core.Extensions;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using static Huybrechts.App.Services.AzurePricingService;

namespace Huybrechts.App.Features.Platform.PlatformRateFlow;

public static class PlatformRateHelper
{
    public static CreateCommand CreateNew(
        PlatformProduct product,
        Ulid? platformRegionId,
        Ulid? platformServiceId,
        string currencyCode
        ) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = product.PlatformInfoId,
        PlatformProductId = product.Id,
        PlatformProductLabel = product.Name,
        PlatformRegionId = platformRegionId ?? Ulid.Empty,
        PlatformServiceId = platformServiceId ?? Ulid.Empty,
        CurrencyCode = currencyCode,
        ValidFrom = DateTime.Today
    };

    public static async Task AddDefaultSetupUnitsAsync(FeatureContext context, PlatformRate rate, CancellationToken token)
    {
        var defaultUnits = await PlatformDefaultUnitFlow.PlatformDefaultUnitHelper.GetDefaultUnitsFor(context, rate, token);
        if (defaultUnits is null || defaultUnits.Count < 1)
            return;

        foreach (var defaultUnit in defaultUnits)
        {
            if (defaultUnit.SetupUnitId is null || defaultUnit.SetupUnitId == Ulid.Empty)
                continue;

            PlatformRateUnit rateUnit = new()
            {
                Id = Ulid.NewUlid(),
                PlatformInfoId = rate.PlatformInfoId,
                PlatformProductId = rate.PlatformProductId,
                PlatformRate = rate,
                PlatformRateId = rate.Id,
                SetupUnit = defaultUnit.SetupUnit!,
                SetupUnitId = defaultUnit.SetupUnit!.Id,
                UnitOfMeasure = rate.UnitOfMeasure,
                UnitFactor = defaultUnit.UnitFactor,
                DefaultValue = defaultUnit.DefaultValue,
                Description = defaultUnit.Description ?? string.Empty,
                SearchIndex = defaultUnit.SearchIndex,
                CreatedDT = DateTime.UtcNow,
            };
            await context.Set<PlatformRateUnit>().AddAsync(rateUnit, token);
        }

        return;
    }

    public static async Task<List<PlatformRegion>> GetRegionsAsync(FeatureContext dbcontext, Ulid platformInfoId, CancellationToken token)
    {
        return await dbcontext.Set<PlatformRegion>()
            .Where(q => q.PlatformInfoId == platformInfoId)
            .OrderBy(o => o.Label)
            .ToListAsync(cancellationToken: token);
    }

    public static async Task<List<PlatformService>> GetServicesAsync(FeatureContext dbcontext, Ulid platformInfoId, CancellationToken token)
    {
        return await dbcontext.Set<PlatformService>()
            .Where(q => q.PlatformInfoId == platformInfoId)
            .OrderBy(o => o.Label)
            .ToListAsync(cancellationToken: token);
    }

    public static async Task<List<SetupCurrency>> GetCurrenciesAsync(FeatureContext dbcontext, CancellationToken token)
    {
        return await dbcontext.Set<SetupCurrency>()
            .OrderBy(o => o.Code)
            .ToListAsync(cancellationToken: token);
    }

    internal static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    internal static Result ProductNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMPRODUCT_ID.Replace("{0}", id.ToString()));

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMRATE_ID.Replace("{0}", id.ToString()));
}

public record Model
{
    public Ulid Id { get; init; }

    public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

    [Display(Name = "Product", ResourceType = typeof(Localization))]
    public Ulid PlatformProductId { get; set; } = Ulid.Empty;

    [Display(Name = "Product", ResourceType = typeof(Localization))]
    public string PlatformProductLabel { get; set; } = string.Empty;

    [Display(Name = "Region", ResourceType = typeof(Localization))]
    public Ulid PlatformRegionId { get; set; }

    [Display(Name = "Region", ResourceType = typeof(Localization))]
    public string PlatformRegionLabel { get; set; } = string.Empty;

    [Display(Name = "Service", ResourceType = typeof(Localization))]
    public Ulid PlatformServiceId { get; set; }

    [Display(Name = "Service", ResourceType = typeof(Localization))]
    public string PlatformServiceLabel { get; set; } = string.Empty;

    [Display(Name = nameof(ServiceName), ResourceType = typeof(Localization))]
    public string ServiceName { get; set; } = string.Empty;

    [Display(Name = nameof(ServiceFamily), ResourceType = typeof(Localization))]
    public string ServiceFamily { get; set; } = string.Empty;

    [Display(Name = nameof(ProductName), ResourceType = typeof(Localization))]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = nameof(SkuName), ResourceType = typeof(Localization))]
    public string SkuName { get; set; } = string.Empty;

    [Display(Name = nameof(MeterName), ResourceType = typeof(Localization))]
    public string MeterName { get; set; } = string.Empty;

    [Display(Name = nameof(CurrencyCode), ResourceType = typeof(Localization))]
    public string CurrencyCode { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = nameof(ValidFrom), ResourceType = typeof(Localization))]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime ValidFrom { get; set; }

    [Precision(12, 6)]
    [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
    [Display(Name = nameof(RetailPrice), ResourceType = typeof(Localization))]
    public decimal RetailPrice { get; set; } = 0;

    [Precision(12, 6)]
    [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
    [Display(Name = nameof(UnitPrice), ResourceType = typeof(Localization))]
    public decimal UnitPrice { get; set; } = 0;

    [Precision(12, 6)]
    [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
    [Display(Name = nameof(MinimumUnits), ResourceType = typeof(Localization))]
    public decimal MinimumUnits { get; set; } = 0;

    [Display(Name = nameof(UnitOfMeasure), ResourceType = typeof(Localization))]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Display(Name = nameof(RateType), ResourceType = typeof(Localization))]
    public string RateType { get; set; } = string.Empty;

    [Display(Name = nameof(IsPrimaryRegion), ResourceType = typeof(Localization))]
    public bool IsPrimaryRegion { get; set; } = false;

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    public string SearchIndex => $"{ServiceName}~{ServiceFamily}~{ProductName}~{SkuName}~{MeterName}".ToLowerInvariant();

    public ICollection<PlatformRateUnit> RateUnits { get; set; } = [];
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformRegionId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformServiceId).NotEmpty().NotEqual(Ulid.Empty);

        RuleFor(m => m.ServiceName).NotEmpty().Length(1, 128);
        RuleFor(m => m.ServiceFamily).NotEmpty().Length(1, 128);
        RuleFor(m => m.ProductName).NotEmpty().Length(1, 128);
        RuleFor(m => m.SkuName).NotEmpty().Length(1, 128);
        RuleFor(m => m.MeterName).NotEmpty().Length(1, 128);
        RuleFor(m => m.RateType).NotEmpty().Length(1, 128);
        RuleFor(m => m.CurrencyCode).NotEmpty().Length(1, 10);
        RuleFor(m => m.ValidFrom).NotEmpty();
        RuleFor(m => m.RetailPrice).NotNull();
        RuleFor(m => m.UnitPrice).NotNull();
        RuleFor(m => m.MinimumUnits).NotNull();
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile
{
    public ListMapping() =>
        CreateProjection<PlatformRate, ListModel>()
        .ForMember(dest => dest.PlatformProductLabel, opt => opt.MapFrom(src => src.PlatformProduct.Label));
}

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>>
{
    public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

    public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

    public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class ListValidator : AbstractValidator<ListQuery> 
{
    public ListValidator() 
    { 
        RuleFor(x => x.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty); 
    } 
}

public sealed record ListResult : EntityFlow.ListResult<ListModel>
{
    public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

    public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

    public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

    public string CurrencyCode { get; set; } = string.Empty;

    public PlatformInfo Platform { get; set; } = new();

    public PlatformProduct Product { get; set; } = new();

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformService> Services { get; set; } = [];

    public IList<string> Currencies { get; set; } = [];
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
        var product = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(q => q.Id == message.PlatformProductId, cancellationToken: token);
        if (product == null)
            return PlatformRateHelper.ProductNotFound(message.PlatformProductId ?? Ulid.Empty);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformRateHelper.PlatformNotFound(message.PlatformProductId ?? Ulid.Empty);

        IQueryable<PlatformRate> query = _dbcontext.Set<PlatformRate>();

        if (message.PlatformRegionId.HasValue && message.PlatformRegionId != Ulid.Empty)
            query = query.Where(q => q.PlatformRegionId == message.PlatformRegionId);

        if (message.PlatformServiceId.HasValue && message.PlatformServiceId != Ulid.Empty)
            query = query.Where(q => q.PlatformServiceId == message.PlatformServiceId);

        if (!string.IsNullOrEmpty(message.CurrencyCode))
            query = query.Where(q => q.CurrencyCode == message.CurrencyCode);

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            string searchFor = searchString.ToLowerInvariant();
            query = query.Where(q =>
                q.PlatformProductId == message.PlatformProductId
                && (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)));
        }
        else
        {
            query = query.Where(q => q.PlatformProductId == message.PlatformProductId);
        }

        if (!string.IsNullOrEmpty(message.SortOrder))
        {
            query = query.OrderBy(message.SortOrder);
        }
        else query = query
            .OrderBy(o => o.ServiceName)
            .ThenBy(o => o.ProductName)
            .ThenBy(o => o.SkuName)
            .ThenBy(o => o.MeterName)
            .ThenBy(o => o.RateType)
            .ThenBy(o => o.CurrencyCode)
            .ThenBy(o => o.ValidFrom);

        var regions = await PlatformRateHelper.GetRegionsAsync(_dbcontext, platform.Id, token);
        var services = await PlatformRateHelper.GetServicesAsync(_dbcontext, platform.Id, token);
        var currencies = await PlatformRateHelper.GetCurrenciesAsync(_dbcontext, token);

        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await query
            .Include(i => i.PlatformProduct)
            .Include(i => i.RateUnits)
            .ThenInclude(j => j.SetupUnit)
            .ProjectTo<ListModel>(_configuration)
            .PaginatedListAsync(pageNumber, pageSize);

        // Get names of regions/services. convert to dictionary for performance
        var regionDict = regions.ToDictionary(r => r.Id, r => r.Label);
        var serviceDict = services.ToDictionary(s => s.Id, s => s.Label);
        results.ForEach(q => 
        {
            q.PlatformRegionLabel = regionDict.TryGetValue(q.PlatformRegionId, out string? value1) ? value1 : string.Empty;
            q.PlatformServiceLabel = serviceDict.TryGetValue(q.PlatformServiceId, out string? value2) ? value2 : string.Empty;
        });

        var model = new ListResult
        {
            PlatformProductId = message.PlatformProductId,
            PlatformRegionId = message.PlatformRegionId,
            PlatformServiceId = message.PlatformServiceId,
            CurrencyCode = message.CurrencyCode,
            CurrentFilter = searchString,
            SearchText = searchString,
            SortOrder = message.SortOrder,
            Platform = platform,
            Product = product,
            Regions = regions.ToList(),
            Services = services.ToList(),
            Currencies = currencies.Select(s => s.Code).ToList(),
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
    public Ulid PlatformProductId { get; set; } = Ulid.Empty;
}

public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
{
    public CreateQueryValidator()
    {
        RuleFor(m => m.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
{
    public PlatformInfo Platform { get; set; } = new();

    public PlatformProduct Product { get; set; } = new();

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformService> Services { get; set; } = [];

    public IList<string> Currencies { get; set; } = [];
}

public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
{
    public CreateCommandValidator(FeatureContext dbContext) : base()
    {
        RuleFor(x => x.PlatformProductId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<PlatformProduct>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PLATFORMPRODUCT_ID.Replace("{0}", m.PlatformProductId.ToString()));
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
        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([message.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.ProductNotFound(message.PlatformProductId);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformRateHelper.PlatformNotFound(product.PlatformInfoId);

        var regions = await PlatformRateHelper.GetRegionsAsync(_dbcontext, platform.Id, token);
        var services = await PlatformRateHelper.GetServicesAsync(_dbcontext, platform.Id, token);
        var currencies = (await PlatformRateHelper.GetCurrenciesAsync(_dbcontext, token)).Select(s => s.Code).ToList();

        var record = PlatformRateHelper.CreateNew(product,
            Ulid.Empty,
            Ulid.Empty, 
            string.Empty);
        record.Platform = platform;
        record.Product = product;
        record.Regions = regions;
        record.Services = services;
        record.Currencies = currencies;

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
        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([message.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.ProductNotFound(message.PlatformProductId);

        var record = new PlatformRate
        {
            Id = message.Id,
            PlatformInfoId = product.PlatformInfoId,
            PlatformProduct = product,
            PlatformRegionId = message.PlatformRegionId,
            PlatformServiceId = message.PlatformServiceId,
            PlatformProductId = message.PlatformProductId,
            Remark = message.Remark?.Trim(),
            SearchIndex = message.SearchIndex?.Trim(),
            CreatedDT = DateTime.UtcNow,

            ServiceName = message.ServiceName.Trim(),
            ServiceFamily = message.ServiceFamily.Trim(),
            ProductName = message.ProductName.Trim(),
            SkuName = message.SkuName.Trim(),
            MeterName = message.MeterName.Trim(),
            RateType = message.RateType.Trim(),
            CurrencyCode = message.CurrencyCode.Trim(),
            ValidFrom = message.ValidFrom,
            RetailPrice = decimal.Round(message.RetailPrice, 6, MidpointRounding.ToEven),
            UnitPrice = decimal.Round(message.UnitPrice, 6, MidpointRounding.ToEven),
            MinimumUnits = decimal.Round(message.MinimumUnits, 6, MidpointRounding.ToEven),
            UnitOfMeasure = message.UnitOfMeasure.Trim(),
            IsPrimaryRegion = message.IsPrimaryRegion
        };
        await _dbcontext.Set<PlatformRate>().AddAsync(record, token);

        await PlatformRateHelper.AddDefaultSetupUnitsAsync(_dbcontext, record, token);

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

    public PlatformProduct Product { get; set; } = new();

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformService> Services { get; set; } = [];

    public IList<string> Currencies { get; set; } = [];
}

public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
{
    public UpdateCommandValidator(FeatureContext dbContext)
    {
        RuleFor(x => x.PlatformProductId).MustAsync(async (id, cancellation) =>
        {
            bool exists = await dbContext.Set<PlatformProduct>().AnyAsync(x => x.Id == id, cancellation);
            return exists;
        })
        .WithMessage(m => Messages.INVALID_PLATFORMPRODUCT_ID.Replace("{0}", m.PlatformProductId.ToString()));
    }
}

internal class UpdateCommandMapping : Profile
{
    public UpdateCommandMapping() => 
        CreateProjection<PlatformRate, UpdateCommand>()
        .ForMember(dest => dest.PlatformProductLabel, opt => opt.MapFrom(src => src.PlatformProduct.Name));
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
        var record = await _dbcontext.Set<PlatformRate>()
            .Include(i => i.PlatformProduct)
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null) 
            return PlatformRateHelper.RecordNotFound(message.Id);

        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([record.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.ProductNotFound(record.PlatformProductId);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformRateHelper.PlatformNotFound(product.PlatformInfoId);

        record.Platform = platform;
        record.Product = product;
        record.Regions = await PlatformRateHelper.GetRegionsAsync(_dbcontext, record.PlatformInfoId, token);
        record.Services = await PlatformRateHelper.GetServicesAsync(_dbcontext, record.PlatformInfoId, token);
        record.Currencies = (await PlatformRateHelper.GetCurrenciesAsync(_dbcontext, token)).Select(s => s.Code).ToList();
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
        var record = await _dbcontext.Set<PlatformRate>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return PlatformRateHelper.RecordNotFound(message.Id);

        record.PlatformRegionId = message.PlatformRegionId;
        record.PlatformServiceId = message.PlatformServiceId;
        record.Remark = message.Remark?.Trim();
        record.SearchIndex = message.SearchIndex?.Trim();
        record.ModifiedDT = DateTime.UtcNow;

        record.ServiceName = message.ServiceName.Trim();
        record.ServiceFamily = message.ServiceFamily.Trim();
        record.ProductName = message.ProductName.Trim();
        record.SkuName = message.SkuName.Trim();
        record.MeterName = message.MeterName.Trim();
        record.RateType = message.RateType.Trim();
        record.CurrencyCode = message.CurrencyCode.Trim();
        record.ValidFrom = message.ValidFrom;
        record.RetailPrice = decimal.Round(message.RetailPrice, 6, MidpointRounding.ToEven);
        record.UnitPrice = decimal.Round(message.UnitPrice, 6, MidpointRounding.ToEven);
        record.MinimumUnits = decimal.Round(message.MinimumUnits, 6, MidpointRounding.ToEven);
        record.UnitOfMeasure = message.UnitOfMeasure.Trim();
        record.IsPrimaryRegion = message.IsPrimaryRegion;

        _dbcontext.Set<PlatformRate>().Update(record);
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

    public PlatformProduct Product { get; set; } = new();

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformService> Services { get; set; } = [];

    public IList<string> Currencies { get; set; } = [];
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
        CreateProjection<PlatformRate, DeleteCommand>()
        .ForMember(dest => dest.PlatformProductLabel, opt => opt.MapFrom(src => src.PlatformProduct.Name));
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
        var record = await _dbcontext.Set<PlatformRate>()
            .Include(i => i.PlatformProduct)
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

        if (record == null)
            return PlatformRateHelper.RecordNotFound(message.Id);

        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([record.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.ProductNotFound(record.PlatformProductId);

        var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
        if (platform == null)
            return PlatformRateHelper.PlatformNotFound(product.PlatformInfoId);

        record.Platform = platform;
        record.Product = product;
        record.Regions = await PlatformRateHelper.GetRegionsAsync(_dbcontext, record.PlatformInfoId, token);
        record.Services = await PlatformRateHelper.GetServicesAsync(_dbcontext, record.PlatformInfoId, token);
        record.Currencies = (await PlatformRateHelper.GetCurrenciesAsync(_dbcontext, token)).Select(s => s.Code).ToList();

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
        var record = await _dbcontext.Set<PlatformRate>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return PlatformRateHelper.RecordNotFound(message.Id);

        _dbcontext.Set<PlatformRate>().Remove(record);
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

internal sealed class ImportModelMapping : Profile
{
    public ImportModelMapping() => CreateMap<PricingItem, ImportModel>()
        .ForMember(dest => dest.IsPrimaryRegion, opt => opt.MapFrom(src => src.IsPrimaryMeterRegion))
        .ForMember(dest => dest.MinimumUnits, opt => opt.MapFrom(src => src.TierMinimumUnits))
        .ForMember(dest => dest.RateType, opt => opt.MapFrom(src => src.Type))
        .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.EffectiveStartDate))
        ;
}

public sealed record ImportQuery : EntityFlow.ListQuery, IRequest<Result<ImportResult>>
{
    public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

    public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

    public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

    public string CurrencyCode { get; set; } = string.Empty;
}

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery>
{
    public ImportQueryValidator()
    {
        RuleFor(m => m.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

public sealed record ImportResult : EntityFlow.ListResult<ImportModel>
{
    public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

    public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

    public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

    public string CurrencyCode { get; set; } = string.Empty;

    public PlatformInfo Platform { get; set; } = new();

    public PlatformProduct Product { get; set; } = new();

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformService> Services { get; set; } = [];

    public IList<string> Currencies { get; set; } = [];
}

public sealed record ImportCommand : IRequest<Result>
{
    public Ulid PlatformProductId { get; set; }

    public Ulid PlatformRegionId { get; set; }

    public Ulid PlatformServiceId { get; set; }

    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand>
{
    public ImportCommandValidator()
    {
        RuleFor(m => m.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformRegionId).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.PlatformServiceId).NotEmpty().NotEqual(Ulid.Empty);
    }
}

internal sealed class ImportQueryHandler :
    EntityFlow.ListHandler<PlatformRate, ImportModel>,
    IRequestHandler<ImportQuery, Result<ImportResult>>
{
    private readonly IMemoryCache _cache;
    private readonly PlatformImportOptions _options;
    private readonly IMapper _mapper;

    public ImportQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options, IMapper mapper, IMemoryCache cache)
        : base(dbcontext, configuration)
    {
        _options = options;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Result<ImportResult>> Handle(ImportQuery message, CancellationToken token)
    {
        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([message.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.ProductNotFound(message.PlatformProductId ?? Ulid.Empty);

        var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([product.PlatformInfoId], cancellationToken: token);
        if (platform is null)
            return PlatformRateHelper.PlatformNotFound(product.PlatformInfoId);

        var regions = await PlatformRateHelper.GetRegionsAsync(_dbcontext, platform.Id, token);
        var services = await PlatformRateHelper.GetServicesAsync(_dbcontext, platform.Id, token);
        var currencies = (await PlatformRateHelper.GetCurrenciesAsync(_dbcontext, token)).Select(s => s.Code).ToList();

        if ((!message.Page.HasValue || message.Page <= 1) 
            && ( message.PlatformRegionId.IsEmpty()
            || message.PlatformServiceId.IsEmpty()
            || string.IsNullOrEmpty(message.CurrencyCode)))
            return Result.Ok(new ImportResult
            {
                PlatformProductId = message.PlatformProductId,
                PlatformRegionId = message.PlatformRegionId,
                PlatformServiceId = message.PlatformServiceId,
                CurrencyCode = message.CurrencyCode,
                CurrentFilter = message.CurrentFilter,
                SearchText = message.SearchText,
                SortOrder = message.SortOrder,
                Platform = platform,
                Product = product,
                Currencies = currencies,
                Regions = regions.ToList(),
                Services = services.ToList(),
                Results = []
            });
    
        var region = regions.First(f => f.Id == message.PlatformRegionId).Name;
        var service = services.First(f => f.Id == message.PlatformServiceId).Name;
        var currency = message.CurrencyCode;

        var searchString = message.SearchText ?? message.CurrentFilter;
        List<ImportModel> records = await GetAzureRatesAsync(platform.Provider, message.PlatformProductId!.Value, region, service, currency, searchString);

        if (!string.IsNullOrEmpty(searchString))
        {
            var searchFor = searchString.ToLowerInvariant();
            records = records.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
        }

        records = [.. records
            .OrderBy(o => o.ProductName)
            .ThenBy(o => o.SkuName)
            .ThenBy(o => o.RateType)
            .ThenBy(o => o.MeterName)
            .ThenBy(o => o.CurrencyCode)
            .ThenBy(o => o.MinimumUnits)
            ];
        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;

        return new ImportResult()
        {
            PlatformProductId = product.Id,
            PlatformRegionId = message.PlatformRegionId,
            PlatformServiceId = message.PlatformServiceId,
            CurrencyCode = currency,
            Platform = platform,
            Product = product,
            Regions = regions,
            Services = services,
            Currencies = currencies,
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

    private async Task<List<ImportModel>> GetAzureRatesAsync(
        PlatformProvider platform,
        Ulid platformProductId,
        string regionName,
        string serviceName,
        string currencyCode,
        string searchString)
    {
        List<ImportModel> result = [];
            
        var service = new AzurePricingService(_options, _cache);
        var pricing = await service.GetRatesAsync(currencyCode, serviceName, regionName, searchString);

        if (pricing is null)
            return [];

        result = _mapper.Map<List<ImportModel>>(pricing.Items);

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

        var product = await _dbcontext.Set<PlatformProduct>().FindAsync([message.PlatformProductId], cancellationToken: token);
        if (product is null)
            return PlatformRateHelper.PlatformNotFound(message.PlatformProductId);

        bool changes = false;
        foreach (var item in message.Items)
        {
            PlatformRate record = new()
            {
                Id = Ulid.NewUlid(),
                PlatformInfoId = product.PlatformInfoId,
                PlatformProduct = product,
                PlatformRegionId = message.PlatformRegionId,
                PlatformServiceId = message.PlatformServiceId,
                Remark = item.Remark,
                SearchIndex = item.SearchIndex,
                CreatedDT = DateTime.UtcNow,

                ServiceName = item.ServiceName,
                ServiceFamily = item.ServiceFamily,
                ProductName = item.ProductName,
                SkuName = item.SkuName,
                MeterName = item.MeterName,
                RateType = item.RateType,
                CurrencyCode = item.CurrencyCode,
                ValidFrom = item.ValidFrom.Date.ToUniversalTime(),
                RetailPrice = item.RetailPrice,
                UnitPrice = item.UnitPrice,
                MinimumUnits = item.MinimumUnits,
                UnitOfMeasure = item.UnitOfMeasure,
                IsPrimaryRegion = item.IsPrimaryRegion
            };
            await _dbcontext.Set<PlatformRate>().AddAsync(record, token);
            changes = true;

            await PlatformRateHelper.AddDefaultSetupUnitsAsync(_dbcontext, record, token);
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}