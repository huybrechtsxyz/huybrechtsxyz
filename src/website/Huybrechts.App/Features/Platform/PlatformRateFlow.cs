using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Services;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using static Huybrechts.App.Services.AzurePricingService;

namespace Huybrechts.App.Features.Platform;

public static class PlatformRateFlow
{
    private static List<string> defaultCurrencies = ["EUR", "USD"];

    public static List<string> DefaultCurrencies { get => defaultCurrencies; set => defaultCurrencies = value; }

    public record Model
    {
        public Ulid Id { get; init; }

        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        [Display(Name = "Service", ResourceType = typeof(Localization))]
        public Ulid PlatformServiceId { get; set; } = Ulid.Empty;

        [Display(Name = "Service", ResourceType = typeof(Localization))]
        public string PlatformServiceName { get; set; } = string.Empty;

        [Display(Name = "Region", ResourceType = typeof(Localization))]
        public Ulid? PlatformRegionId { get; set; }

        [Display(Name = "Product", ResourceType = typeof(Localization))]
        public Ulid? PlatformProductId { get; set; }

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
        [Display(Name = nameof(MininumUnits), ResourceType = typeof(Localization))]
        public decimal MininumUnits { get; set; } = 0;

        [Display(Name = nameof(UnitOfMeasure), ResourceType = typeof(Localization))]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [Display(Name = nameof(RateType), ResourceType = typeof(Localization))]
        public string RateType { get; set; } = string.Empty;

        [Display(Name = nameof(IsPrimaryRegion), ResourceType = typeof(Localization))]
        public bool IsPrimaryRegion { get; set; } = false;

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEmpty();
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
            RuleFor(m => m.PlatformServiceId).NotNull().NotEmpty();
            RuleFor(m => m.PlatformRegionId).NotNull().NotEmpty();
            RuleFor(m => m.PlatformProductId).NotNull().NotEmpty();

            RuleFor(m => m.ServiceName).NotNull().NotEmpty().Length(1, 128);
            RuleFor(m => m.ServiceFamily).NotNull().Length(1, 128);
            RuleFor(m => m.ProductName).NotNull().NotEmpty().Length(1, 128);
            RuleFor(m => m.SkuName).NotNull().NotEmpty().Length(1, 128);
            RuleFor(m => m.MeterName).NotNull().NotEmpty().Length(1, 128);
            RuleFor(m => m.RateType).NotNull().NotEmpty().Length(1, 128);
            RuleFor(m => m.CurrencyCode).NotNull().NotEmpty().Length(1, 10);
            RuleFor(m => m.ValidFrom).NotNull().NotEmpty();
            RuleFor(m => m.RetailPrice).NotNull();
            RuleFor(m => m.UnitPrice).NotNull();
            RuleFor(m => m.MininumUnits).NotNull();
            RuleFor(m => m.ServiceFamily).Length(1, 128);

        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result ServiceNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMSERVICE_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMRATE_ID.Replace("{0}", id.ToString()));

    //
    // LIST
    //

    public sealed record ListModel : Model
    {
    }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformRate, ListModel>()
            .ForMember(dest => dest.PlatformServiceName, opt => opt.MapFrom(src => src.PlatformService.Name));
    }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<ListResult>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;
    }

    internal sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

        public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

        public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public IList<PlatformRegion>? Regions = null;

        public IList<PlatformProduct>? Products = null;

        public IList<string> Currencies = DefaultCurrencies;
    }

    internal sealed class ListHandler :
        EntityListFlow.Handler<PlatformService, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<ListResult> Handle(ListQuery request, CancellationToken token)
        {
            IQueryable<PlatformRate> query = _dbcontext.Set<PlatformRate>();
            PlatformService? service = null!;

            service = await _dbcontext.Set<PlatformService>().FindAsync([request.PlatformServiceId], cancellationToken: token);
            if (service is null)
                return new ListResult()
                {
                    PlatformInfoId = request.PlatformInfoId,
                    PlatformServiceId = request.PlatformServiceId,
                    CurrentFilter = request.CurrentFilter,
                    SearchText = request.SearchText,
                    SortOrder = request.SortOrder,
                    Regions = [],
                    Products = [],
                    Currencies = DefaultCurrencies,
                    Results = []
                };
            query = query.Where(q => q.PlatformServiceId == request.PlatformServiceId);
            request.PlatformInfoId = service.PlatformInfoId;

            var searchString = request.SearchText ?? request.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q =>
                    q.ServiceName.Contains(searchString)
                    || q.ServiceFamily.Contains(searchString)
                    || q.ProductName.Contains(searchString)
                    || q.SkuName.Contains(searchString)
                    || q.MeterName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(request.SortOrder))
            {
                query = query.OrderBy(request.SortOrder);
            }
            else query = query
                    .OrderBy(o => o.ServiceName)
                    .ThenBy(o => o.ProductName)
                    .ThenBy(o => o.SkuName)
                    .ThenBy(o => o.MeterName)
                    .ThenBy(o => o.RateType)
                    .ThenBy(o => o.CurrencyCode)
                    .ThenBy(o => o.ValidFrom);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;
            var results = await query
                .Include(i => i.PlatformService)
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var regions = await _dbcontext.Set<PlatformRegion>()
                .Where(q => q.PlatformInfoId == service.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            var products = await _dbcontext.Set<PlatformProduct>()
                .Where(q => q.PlatformInfoId == service.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            var model = new ListResult
            {
                PlatformInfoId = request.PlatformInfoId,
                PlatformServiceId = request.PlatformServiceId,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Regions = regions,
                Products = products,
                Currencies = DefaultCurrencies,
                Results = results ?? []
            };

            return model;
        }
    }

    //
    // CREATE
    //

    public static CreateCommand CreateNew(
        Ulid platformInfoId,
        Ulid platformServiceId) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = platformInfoId,
        PlatformServiceId = platformServiceId
    };

    public sealed record CreateQuery : IRequest<Result<CreateResult>>
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid PlatformServiceId { get; set; } = Ulid.Empty;
    }

    internal sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
    {
        public CreateQueryValidator()
        {
            RuleFor(m => m.PlatformServiceId).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public record CreateResult
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid PlatformServiceId { get; set; } = Ulid.Empty;

        public CreateCommand Item { get; set; } = new();

        public IList<PlatformRegion> Regions { get; set; } = [];

        public IList<PlatformProduct> Products { get; set; } = [];

        public IList<string> Currencies = DefaultCurrencies;
    }

    internal class CreateQueryHandler : IRequestHandler<CreateQuery, Result<CreateResult>>
    {
        private readonly PlatformContext _dbcontext;

        public CreateQueryHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<CreateResult>> Handle(CreateQuery request, CancellationToken token)
        {
            var service = await _dbcontext.Set<PlatformService>().FindAsync([request.PlatformServiceId], cancellationToken: token);
            if (service is null)
                return ServiceNotFound(request.PlatformServiceId);

            IList<PlatformRegion> regions = await _dbcontext.Set<PlatformRegion>()
                .Where(q => q.PlatformInfoId == service.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(token);

            IList<PlatformProduct> products = await _dbcontext.Set<PlatformProduct>()
                .Where(q => q.PlatformInfoId == service.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(token);

            return Result.Ok(new CreateResult()
            {
                PlatformInfoId = service.PlatformInfoId,
                PlatformServiceId = service.Id,
                Currencies = DefaultCurrencies,
                Item = CreateNew(service.PlatformInfoId, service.Id),
                Regions = regions,
                Products = products
            });
        }
    }

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
    {
    }

    internal sealed class CreateCommandValidator : ModelValidator<CreateCommand>
    {
        public CreateCommandValidator() : base()
        {
        }
    }

    internal sealed class CreateCommandHandler : IRequestHandler<CreateCommand, Result<Ulid>>
    {
        private readonly PlatformContext _dbcontext;

        public CreateCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<Ulid>> Handle(CreateCommand request, CancellationToken token)
        {
            var service = await _dbcontext.Set<PlatformService>().FindAsync([request.PlatformServiceId], cancellationToken: token);
            if (service is null)
                return ServiceNotFound(request.PlatformServiceId);

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([service.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(service.PlatformInfoId);

            PlatformRate record = new()
            {
                Id = request.Id,
                PlatformInfoId = platform.Id,
                PlatformService = service,
                PlatformRegionId = request.PlatformRegionId,
                PlatformProductId = request.PlatformProductId,
                Remark = request.Remark,
                CreatedDT = DateTime.UtcNow,

                ServiceName = request.ServiceName,
                ServiceFamily = request.ServiceFamily,
                ProductName = request.ProductName,
                SkuName = request.SkuName,
                MeterName = request.MeterName,
                RateType = request.RateType,
                CurrencyCode = request.CurrencyCode,
                ValidFrom = request.ValidFrom,
                RetailPrice = request.RetailPrice,
                UnitPrice = request.UnitPrice,
                MininumUnits = request.MininumUnits,
                UnitOfMeasure = request.UnitOfMeasure,
                IsPrimaryRegion = request.IsPrimaryRegion
            };

            await _dbcontext.Set<PlatformRate>().AddAsync(record, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(record.Id);
        }
    }

    //
    // UPDATE
    //

    public sealed record UpdateQuery : IRequest<Result<UpdateCommand>>
    {
        public Ulid Id { get; init; }
    }

    internal sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
    {
        public UpdateQueryValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEqual(Ulid.Empty);
        }
    }

    public record UpdateCommand : Model, IRequest<Result>
    {
        public IList<PlatformRegion> Regions { get; set; } = [];

        public IList<PlatformProduct> Products { get; set; } = [];

        public IList<string> Currencies = DefaultCurrencies;
    }

    internal class UpdateCommandValidator : ModelValidator<UpdateCommand>
    {
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformRate, UpdateCommand>()
            .ForMember(dest => dest.PlatformServiceName, opt => opt.MapFrom(src => src.PlatformService.Name));
    }

    internal class UpdateQueryHandler : IRequestHandler<UpdateQuery, Result<UpdateCommand>>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public UpdateQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<UpdateCommand>> Handle(UpdateQuery request, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRate>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformService)
                .ProjectTo<UpdateCommand>(_configuration)
                .SingleOrDefaultAsync(token);
            if (record == null) 
                return RecordNotFound(request.Id);

            record.Regions = await _dbcontext.Set<PlatformRegion>()
                .Where(q => q.PlatformInfoId == record.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            record.Products = await _dbcontext.Set<PlatformProduct>()
                .Where(q => q.PlatformInfoId == record.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            record.Currencies = DefaultCurrencies;

            return Result.Ok(record);
        }
    }

    internal class UpdateCommandHandler : IRequestHandler<UpdateCommand, Result>
    {
        private readonly PlatformContext _dbcontext;

        public UpdateCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(UpdateCommand command, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRate>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            record.Remark = command.Remark;
            record.ModifiedDT = DateTime.UtcNow;

            record.ServiceName = command.ServiceName;
            record.ServiceFamily = command.ServiceFamily;
            record.ProductName = command.ProductName;
            record.SkuName = command.SkuName;
            record.MeterName = command.MeterName;
            record.RateType = command.RateType;
            record.CurrencyCode = command.CurrencyCode;
            record.ValidFrom = command.ValidFrom;
            record.RetailPrice = command.RetailPrice;
            record.UnitPrice = command.UnitPrice;
            record.MininumUnits = command.MininumUnits;
            record.UnitOfMeasure = command.UnitOfMeasure;
            record.IsPrimaryRegion = command.IsPrimaryRegion;
            record.PlatformProductId = command.PlatformProductId;
            record.PlatformRegionId = command.PlatformRegionId;

            _dbcontext.Set<PlatformRate>().Update(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // DELETE
    //

    public sealed record DeleteQuery : IRequest<Result<DeleteCommand>>
    {
        public Ulid Id { get; init; }
    }

    internal class DeleteQueryValidator : AbstractValidator<DeleteQuery>
    {
        public DeleteQueryValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public sealed record DeleteCommand : Model, IRequest<Result>
    {
        public IList<PlatformRegion> Regions { get; set; } = [];

        public IList<PlatformProduct> Products { get; set; } = [];

        public IList<string> Currencies = DefaultCurrencies;
    }

    internal sealed class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    internal sealed class DeleteCommandMapping : Profile
    {
        public DeleteCommandMapping() => 
            CreateProjection<PlatformRate, DeleteCommand>()
            .ForMember(dest => dest.PlatformServiceName, opt => opt.MapFrom(src => src.PlatformService.Name));
    }

    internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, Result<DeleteCommand>>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public DeleteQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<Result<DeleteCommand>> Handle(DeleteQuery request, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRate>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformService)
                .ProjectTo<DeleteCommand>(_configuration)
                .SingleOrDefaultAsync(token);
            if (record == null)
                return RecordNotFound(request.Id);

            record.Regions = await _dbcontext.Set<PlatformRegion>()
                .Where(q => q.PlatformInfoId == record.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            record.Products = await _dbcontext.Set<PlatformProduct>()
                .Where(q => q.PlatformInfoId == record.PlatformInfoId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken: token);

            record.Currencies = DefaultCurrencies;

            return Result.Ok(record);
        }
    }

    internal class DeleteCommandHandler : IRequestHandler<DeleteCommand, Result>
    {
        private readonly PlatformContext _dbcontext;

        public DeleteCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(DeleteCommand command, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRate>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            _dbcontext.Set<PlatformRate>().Remove(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // IMPORT
    //

    public sealed record ImportModel
    {
        [Display(Name = nameof(IsSelected), ResourceType = typeof(Localization))]
        public bool IsSelected { get; set; }

        /// <summary>
        /// "currencyCode":"USD"
        /// </summary>
        [Display(Name = nameof(CurrencyCode), ResourceType = typeof(Localization))]
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// "tierMinimumUnits":0.0
        /// </summary>
        [Precision(12, 6)]
        [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
        [Display(Name = nameof(TierMinimumUnits), ResourceType = typeof(Localization))]
        public decimal TierMinimumUnits { get; set; }

        /// <summary>
        /// "retailPrice":0.29601
        /// </summary>
        [Precision(12, 6)]
        [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
        [Display(Name = nameof(RetailPrice), ResourceType = typeof(Localization))]
        public decimal RetailPrice { get; set; }

        /// <summary>
        /// "unitPrice":0.29601
        /// </summary>
        [Precision(12, 6)]
        [DisplayFormat(DataFormatString = "{0:F6}", ApplyFormatInEditMode = true)]
        [Display(Name = nameof(UnitPrice), ResourceType = typeof(Localization))]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// "armRegionName":"southindia"
        /// </summary>
        [Display(Name = nameof(ArmRegionName), ResourceType = typeof(Localization))]
        public string? ArmRegionName { get; set; }

        /// <summary>
        /// "location":"IN South"
        /// </summary>
        [Display(Name = nameof(Location), ResourceType = typeof(Localization))]
        public string? Location { get; set; }

        /// <summary>
        /// "":"2024-08-01T00:00:00Z"
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = nameof(EffectiveStartDate), ResourceType = typeof(Localization))]
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// "meterId":"000009d0-057f-5f2b-b7e9-9e26add324a8"
        /// </summary>
        [Display(Name = nameof(MeterId), ResourceType = typeof(Localization))]
        public string? MeterId { get; set; }

        /// <summary>
        /// "meterName":"D14/DS14 Spot"
        /// </summary>
        [Display(Name = nameof(MeterName), ResourceType = typeof(Localization))]
        public string? MeterName { get; set; }

        /// <summary>
        /// "productId":"DZH318Z0BPVW"
        /// </summary>
        [Display(Name = nameof(ProductId), ResourceType = typeof(Localization))]
        public string? ProductId { get; set; }

        /// <summary>
        /// "productName":"Virtual Machines D Series Windows"
        /// </summary>
        [Display(Name = nameof(ProductName), ResourceType = typeof(Localization))]
        public string? ProductName { get; set; }

        /// <summary>
        /// "skuId":"DZH318Z0BPVW/00QZ"
        /// </summary>
        [Display(Name = nameof(SkuId), ResourceType = typeof(Localization))]
        public string? SkuId { get; set; }

        /// <summary>
        /// "skuName":"D14 Spot"
        /// </summary>
        [Display(Name = nameof(SkuName), ResourceType = typeof(Localization))]
        public string? SkuName { get; set; }

        /// <summary>
        /// "armSkuName":"Standard_D14"
        /// </summary>
        [Display(Name = nameof(ArmSkuName), ResourceType = typeof(Localization))]
        public string? ArmSkuName { get; set; }

        /// <summary>
        /// "serviceId":"DZH313Z7MMC8"
        /// </summary>
        [Display(Name = nameof(ServiceId), ResourceType = typeof(Localization))]
        public string? ServiceId { get; set; }

        /// <summary>
        /// "serviceFamily":"Compute"
        /// </summary>
        [Display(Name = nameof(ServiceFamily), ResourceType = typeof(Localization))]
        public string? ServiceFamily { get; set; }

        /// <summary>
        /// "serviceName":"Virtual Machines"
        /// </summary>
        [Display(Name = nameof(ServiceName), ResourceType = typeof(Localization))]
        public string? ServiceName { get; set; }

        /// <summary>
        /// "unitOfMeasure":"1 Hour"
        /// </summary>
        [Display(Name = nameof(UnitOfMeasure), ResourceType = typeof(Localization))]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// "type":"Consumption"
        /// </summary>
        [Display(Name = nameof(RateType), ResourceType = typeof(Localization))]
        public string? RateType { get; set; }

        /// <summary>
        /// "isPrimaryMeterRegion": true
        /// </summary>
        [Display(Name = nameof(IsPrimaryRegion), ResourceType = typeof(Localization))]
        public bool IsPrimaryRegion { get; set; }
    }

    public sealed class ImportQuery : EntityListFlow.Query, IRequest<ImportResult>
    {
        public Ulid PlatformServiceId { get; set; } = Ulid.Empty;

        public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

        public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

        public string Currency { get; set; } = string.Empty;
    }

    internal sealed class ImportQueryValidator : AbstractValidator<ImportQuery>
    {
        public ImportQueryValidator()
        {
            RuleFor(m => m.PlatformServiceId).NotNull().NotEmpty();
        }
    }

    public sealed class ImportResult : EntityListFlow.Result<ImportModel>
    {
        public Ulid? PlatformServiceId { get; set; } = Ulid.Empty;

        public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

        public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

        public string Currency { get; set; } = string.Empty;

        public IList<PlatformRegion> Regions { get; set; } = [];

        public IList<PlatformProduct> Products { get; set; } = [];

        public IList<string> Currencies = DefaultCurrencies;
    }

    internal sealed class ImportResultMapping : Profile
    {
        public ImportResultMapping() =>
            CreateMap<PricingItem, ImportModel>();
    }

    public sealed record ImportCommand : IRequest<Result>
    {
        public Ulid PlatformServiceId { get; set; }

        public Ulid PlatformRegionId { get; set; }

        public Ulid PlatformProductId { get; set; }

        public string Currency { get; set; } = string.Empty;

        public List<ImportModel> Items { get; set; } = [];
    }

    internal sealed class ImportCommandValidator : AbstractValidator<ImportCommand>
    {
        public ImportCommandValidator()
        {
            RuleFor(m => m.PlatformServiceId).NotNull();
        }
    }

    internal sealed class ImportQueryHandler :
       EntityListFlow.Handler<PlatformRate, ImportModel>,
       IRequestHandler<ImportQuery, ImportResult>
    {
        private readonly PlatformImportOptions _options;
        private readonly IMapper _mapper;

        public ImportQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options, IMapper mapper)
            : base(dbcontext, configuration)
        {
            _options = options;
            _mapper = mapper;
        }

        public async Task<ImportResult> Handle(ImportQuery request, CancellationToken token)
        {
            var service = await _dbcontext.Set<PlatformService>().FindAsync([request.PlatformServiceId], cancellationToken: token);
            if (service is null)
                return new ImportResult()
                {
                    PlatformServiceId = request.PlatformServiceId,
                    PlatformRegionId = request.PlatformRegionId,
                    PlatformProductId = request.PlatformProductId,
                    CurrentFilter = request.CurrentFilter,
                    SearchText = request.SearchText,
                    SortOrder = request.SortOrder,
                    Regions = [],
                    Products = [],
                    Currencies = DefaultCurrencies,
                    Results = []
                };

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([service.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return new ImportResult()
                {
                    PlatformServiceId = request.PlatformServiceId,
                    PlatformRegionId = request.PlatformRegionId,
                    PlatformProductId = request.PlatformProductId,
                    CurrentFilter = request.CurrentFilter,
                    SearchText = request.SearchText,
                    SortOrder = request.SortOrder,
                    Regions = [],
                    Products = [],
                    Currencies = DefaultCurrencies,
                    Results = []
                };

            var regions = _dbcontext.Set<PlatformRegion>().Where(q => q.PlatformInfoId == platform.Id).OrderBy(o => o.Name).ToList();
            var regionName = (regions.FirstOrDefault(f => f.Id == request.PlatformRegionId) ?? new()).Name;

            var products = _dbcontext.Set<PlatformProduct>().Where(q => q.PlatformInfoId == platform.Id).OrderBy(o => o.Name).ToList();
            var productName = (products.FirstOrDefault(f => f.Id == request.PlatformProductId) ?? new()).Name;

            var searchString = request.SearchText ?? request.CurrentFilter;
            List<ImportModel> rates = await GetAzureRatesAsync(
                platform.Provider.ToString(),
                service.PlatformInfoId,
                "EUR", //currency
                productName, //service
                regionName, //location
                searchString);

            rates = [.. rates
                .OrderBy(o => o.ProductName)
                .ThenBy(o => o.SkuName)
                .ThenBy(o => o.RateType)
                .ThenBy(o => o.MeterName)
                .ThenBy(o => o.Location)
                .ThenBy(o => o.CurrencyCode)
                .ThenBy(o => o.TierMinimumUnits)
                ];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;

            var result = new ImportResult()
            {
                PlatformServiceId = request.PlatformServiceId,
                PlatformRegionId = request.PlatformRegionId,
                PlatformProductId = request.PlatformProductId,
                Currency = request.Currency,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Regions = regions,
                Products = products,
                Currencies = DefaultCurrencies,
                Results = new PaginatedList<ImportModel>(
                    rates.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                    rates.Count,
                    pageNumber,
                    pageSize)
            };
            return result;
        }

        private async Task<List<ImportModel>> GetAzureRatesAsync(
            string platform,
            Ulid platformInfoId,
            string currency,
            string serviceName,
            string location,
            string searchString)
        {
            List<ImportModel> result = [];

            var service = new AzurePricingService(_options);
            var pricing = await service.GetRatesAsync(currency, serviceName, location, searchString);

            if (pricing is null)
                return [];

            result = _mapper.Map<List<ImportModel>>(pricing.Items);

            return result;
        }
    }

    internal class ImportCommandHandler : IRequestHandler<ImportCommand, Result>
    {
        private readonly PlatformContext _dbcontext;

        public ImportCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(ImportCommand command, CancellationToken token)
        {
            if (command is null || command.Items is null || command.Items.Count < 0)
                return Result.Ok();

            var service = await _dbcontext.Set<PlatformService>().FindAsync([command.PlatformServiceId], cancellationToken: token);
            if (service is null)
                return ServiceNotFound(command.PlatformServiceId);

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([service.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(service.PlatformInfoId);

            bool changes = false;
            foreach (var item in command.Items)
            {
                PlatformRate record = new()
                {
                    Id = Ulid.NewUlid(),
                    PlatformInfoId = platform.Id,
                    PlatformService = service,
                    PlatformRegionId = command.PlatformRegionId,
                    PlatformProductId = command.PlatformProductId,
                    Remark = null,
                    CreatedDT = DateTime.UtcNow,

                    ServiceName = item.ServiceName ?? string.Empty,
                    ServiceFamily = item.ServiceFamily ?? string.Empty,
                    ProductName = item.ProductName ?? string.Empty,
                    SkuName = item.SkuName ?? string.Empty,
                    MeterName = item.MeterName ?? string.Empty,
                    RateType = item.RateType ?? string.Empty,
                    CurrencyCode = item.CurrencyCode,
                    ValidFrom = item.EffectiveStartDate ?? DateTime.Today,
                    RetailPrice = item.RetailPrice,
                    UnitPrice = item.UnitPrice,
                    MininumUnits = item.TierMinimumUnits,
                    UnitOfMeasure = item.UnitOfMeasure ?? string.Empty,
                    IsPrimaryRegion = item.IsPrimaryRegion
                };

                await _dbcontext.Set<PlatformRate>().AddAsync(record, token);
                changes = true;
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
