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
using StackExchange.Profiling.Internal;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

using static Huybrechts.App.Services.AzurePricingService;

namespace Huybrechts.App.Features.Platform;

public static class PlatformServiceFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = "Platform", ResourceType = typeof(Localization))]
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        [Display(Name = "Platform", ResourceType = typeof(Localization))]
        public string PlatformInfoName { get; set; } = string.Empty;

        [Display(Name = "Region", ResourceType = typeof(Localization))]
        public Ulid PlatformRegionId { get; set; } = Ulid.Empty;

        [Display(Name = "Region", ResourceType = typeof(Localization))]
        public string PlatformRegionName { get; set; } = string.Empty; 
        
        [Display(Name = "Product", ResourceType = typeof(Localization))]
        public Ulid PlatformProductId { get; set; } = Ulid.Empty;

        [Display(Name = "Product", ResourceType = typeof(Localization))]
        public string PlatformProductName { get; set; } = string.Empty;

        [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Label), ResourceType = typeof(Localization))]
        public string Label { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(CostDriver), ResourceType = typeof(Localization))]
        public string? CostDriver { get; set; }

        [Display(Name = nameof(CostBasedOn), ResourceType = typeof(Localization))]
        public string? CostBasedOn { get; set; }

        [Display(Name = nameof(Limitations), ResourceType = typeof(Localization))]
        public string? Limitations { get; set; }

        [Display(Name = nameof(AboutURL), ResourceType = typeof(Localization))]
        public string? AboutURL { get; set; }

        [Display(Name = nameof(PricingURL), ResourceType = typeof(Localization))]
        public string? PricingURL { get; set; }

        [Display(Name = nameof(ServiceId), ResourceType = typeof(Localization))]
        public string? ServiceId { get; set; }

        [Display(Name = nameof(ServiceName), ResourceType = typeof(Localization))]
        public string? ServiceName { get; set; }

        [Display(Name = nameof(Size), ResourceType = typeof(Localization))]
        public string? Size { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEmpty();
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
            RuleFor(m => m.Name).NotNull().Length(1, 128);
            RuleFor(m => m.Label).Length(1, 128);
            RuleFor(m => m.Description).Length(1, 256);

            RuleFor(m => m.CostDriver).Length(1, 256);
            RuleFor(m => m.CostBasedOn).Length(1, 128);
            RuleFor(m => m.Limitations).Length(1, 512);
            RuleFor(m => m.AboutURL).Length(1, 512);
            RuleFor(m => m.PricingURL).Length(1, 512);
            RuleFor(m => m.ServiceId).Length(1, 64);
            RuleFor(m => m.ServiceName).Length(1, 64);
            RuleFor(m => m.Size).Length(1, 128);
        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMSERVICE_ID.Replace("{0}", id.ToString()));

    //
    // LIST
    //

    public sealed record ListModel : Model
    {
    }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformService, ListModel>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
    }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<ListResult>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;
    }

    internal sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public IList<PlatformInfo>? Platforms = null;
    }

    internal sealed class ListHandler :
        EntityListFlow.Handler<PlatformService, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<ListResult> Handle(ListQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlatformService> query = _dbcontext.Set<PlatformService>();

            if (request.PlatformInfoId.HasValue)
            {
                query = query.Where(q => q.PlatformInfoId == request.PlatformInfoId);
            }

            var searchString = request.SearchText ?? request.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q =>
                    q.Name.Contains(searchString)
                    || q.Label.Contains(searchString)
                    || (q.Description.HasValue() && q.Description!.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(request.SortOrder))
            {
                query = query.OrderBy(request.SortOrder);
            }
            else query = query.OrderBy(o => o.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;
            var results = await query
                .Include(i => i.PlatformInfo)
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var platforms = _dbcontext.Set<PlatformInfo>().OrderBy(o => o.Name).ToList();

            var model = new ListResult
            {
                PlatformInfoId = request.PlatformInfoId,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Platforms = platforms,
                Results = results ?? []
            };

            return model;
        }
    }

    //
    // CREATE
    //

    public static CreateCommand CreateNew(Ulid? platformInfoId) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = platformInfoId ?? Ulid.Empty
    };

    public sealed record CreateQuery : IRequest<Result<CreateResult>>
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;
    }

    internal sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
    {
        public CreateQueryValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public record CreateResult
    {
        public CreateCommand Service { get; set; } = new();

        public IList<PlatformInfo> Platforms { get; set; } = [];
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
            IList<PlatformInfo> platforms = await _dbcontext.Set<PlatformInfo>().ToListAsync(cancellationToken: token);

            var platform = platforms.FirstOrDefault(f => f.Id == request.PlatformInfoId);
            if (platform is null)
                return PlatformNotFound(request.PlatformInfoId);

            return Result.Ok(new CreateResult()
            {
                Service = CreateNew(request.PlatformInfoId),
                Platforms = platforms
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
            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([request.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(request.PlatformInfoId);

            var record = new PlatformService
            {
                Id = request.Id,
                PlatformInfo = platform,
                Name = request.Name,
                Description = request.Description,
                Label = request.Label,
                Remark = request.Remark,
                CreatedDT = DateTime.UtcNow,

                PlatformRegionId = request.PlatformRegionId,
                PlatformProductId = request.PlatformProductId,
                CostDriver = request.CostDriver,
                CostBasedOn = request.CostBasedOn,
                Limitations = request.Limitations,
                AboutURL = request.AboutURL,
                PricingURL = request.PricingURL,
                ServiceId = request.ServiceId,
                ServiceName = request.ServiceName,
                Size = request.Size
            };

            await _dbcontext.Set<PlatformService>().AddAsync(record, token);
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
        public IList<PlatformInfo> Platforms { get; set; } = [];
    }

    internal class UpdateCommandValidator : ModelValidator<UpdateCommand>
    {
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformService, UpdateCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
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
            var record = await _dbcontext.Set<PlatformService>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformInfo)
                .ProjectTo<UpdateCommand>(_configuration)
                .SingleOrDefaultAsync(token);
            if (record == null) 
                return RecordNotFound(request.Id);
            record.Platforms = await _dbcontext.Set<PlatformInfo>().OrderBy(o => o.Name).ToListAsync(cancellationToken: token);
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
            var record = await _dbcontext.Set<PlatformService>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            record.Name = command.Name;
            record.Description = command.Description;
            record.Label = command.Label;
            record.Remark = command.Remark;
            record.ModifiedDT = DateTime.UtcNow;

            record.PlatformRegionId = command.PlatformRegionId;
            record.PlatformProductId = command.PlatformProductId;
            record.CostDriver = command.CostDriver;
            record.CostBasedOn = command.CostBasedOn;
            record.Limitations = command.Limitations;
            record.AboutURL = command.AboutURL;
            record.PricingURL = command.PricingURL;
            record.ServiceId = command.ServiceId;
            record.ServiceName = command.ServiceName;
            record.Size = command.Size;

            _dbcontext.Set<PlatformService>().Update(record);
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
        public IList<PlatformInfo> Platforms { get; set; } = [];
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
            CreateProjection<PlatformService, DeleteCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
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
            var record = await _dbcontext.Set<PlatformService>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformInfo)
                .ProjectTo<DeleteCommand>(_configuration)
                .SingleOrDefaultAsync(token);
            if (record == null)
                return RecordNotFound(request.Id);
            record.Platforms = await _dbcontext.Set<PlatformInfo>().OrderBy(o => o.Name).ToListAsync(cancellationToken: token);
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
            var record = await _dbcontext.Set<PlatformService>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            _dbcontext.Set<PlatformService>().Remove(record);
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
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// "tierMinimumUnits":0.0
        /// </summary>
        [Display(Name = nameof(TierMinimumUnits), ResourceType = typeof(Localization))]
        public double TierMinimumUnits { get; set; }

        /// <summary>
        /// "retailPrice":0.29601
        /// </summary>
        [Display(Name = nameof(RetailPrice), ResourceType = typeof(Localization))]
        public double RetailPrice { get; set; }

        /// <summary>
        /// "unitPrice":0.29601
        /// </summary>
        [Display(Name = nameof(UnitPrice), ResourceType = typeof(Localization))]
        public double UnitPrice { get; set; }

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
        [Display(Name = nameof(EffectiveStartDate), ResourceType = typeof(Localization))]
        public string? EffectiveStartDate { get; set; }

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
        [Display(Name = nameof(Type), ResourceType = typeof(Localization))]
        public string? Type { get; set; }

        /// <summary>
        /// "isPrimaryMeterRegion": true
        /// </summary>
        [Display(Name = nameof(IsPrimaryMeterRegion), ResourceType = typeof(Localization))]
        public bool IsPrimaryMeterRegion { get; set; }

        
    }

    public sealed class ImportQuery : EntityListFlow.Query, IRequest<ImportResult>
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

        public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

        public string CurrencyCode { get; set; } = string.Empty;
    }

    internal sealed class ImportQueryValidator : AbstractValidator<ImportQuery>
    {
        public ImportQueryValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
        }
    }

    public sealed class ImportResult : EntityListFlow.Result<ImportModel>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public Ulid? PlatformRegionId { get; set; } = Ulid.Empty;

        public Ulid? PlatformProductId { get; set; } = Ulid.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public List<PlatformInfo> Platforms { get; set; } = [];

        public List<string> Currencies { get; set; } = ["EUR", "USD"];

        public List<PlatformRegion> Regions { get; set; } = [];

        public List<PlatformProduct> Products { get; set; } = [];
    }

    internal sealed class ImportResultMapping : Profile
    {
        public ImportResultMapping() =>
            CreateMap<PricingItem, ImportModel>();
    }

    public sealed record ImportCommand : IRequest<Result>
    {
        public Ulid PlatformInfoId { get; set; }

        public List<ImportModel> Items { get; set; } = [];
    }

    internal sealed class ImportCommandValidator : AbstractValidator<ImportCommand>
    {
        public ImportCommandValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotNull();
        }
    }

    internal sealed class ImportQueryHandler :
       EntityListFlow.Handler<PlatformService, ImportModel>,
       IRequestHandler<ImportQuery, ImportResult>
    {
        private readonly PlatformImportOptions _options;
        private readonly IMapper _mapper;

        public ImportQueryHandler(
            PlatformContext dbcontext,
            IMapper mapper,
            IConfigurationProvider configuration, 
            PlatformImportOptions options)
            : base(dbcontext, configuration)
        {
            _options = options;
            _mapper = mapper;
        }

        public async Task<ImportResult> Handle(ImportQuery request, CancellationToken token)
        {
            var platforms = _dbcontext.Set<PlatformInfo>().OrderBy(o => o.Name).ToList();
            
            var platform = platforms.FirstOrDefault(f => f.Id == request.PlatformInfoId);
            if (platform is null)
                return new ImportResult()
                {
                    PlatformInfoId = Ulid.Empty,
                    CurrentFilter = request.CurrentFilter,
                    SearchText = request.SearchText,
                    SortOrder = request.SortOrder,
                    Results = []
                };

            var regions = _dbcontext.Set<PlatformRegion>().Where(q => q.PlatformInfoId == platform.Id).OrderBy(o => o.Name).ToList();
            var products = _dbcontext.Set<PlatformProduct>().Where(q => q.PlatformInfoId == platform.Id).OrderBy(o => o.Name).ToList();
            var searchString = request.SearchText ?? request.CurrentFilter;

            if (string.IsNullOrEmpty(request.CurrencyCode))
            {
                return new ImportResult()
                {
                    PlatformInfoId = request.PlatformInfoId,
                    CurrentFilter = searchString,
                    SearchText = searchString,
                    SortOrder = request.SortOrder,
                    Platforms = platforms,
                    Regions = regions,
                    Products = products,
                    Results = []
                };
            }

            var locationName = (regions.FirstOrDefault(f => f.Id == request.PlatformRegionId) ?? new()).Name;
            var productName = (products.FirstOrDefault(f => f.Id == request.PlatformProductId) ?? new()).Name;
            
            List<ImportModel> rates = await GetAzureRatesAsync(
                platform.Provider.ToString(),
                request.PlatformInfoId,
                request.CurrencyCode,
                productName,
                locationName,
                searchString);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                //rates = rates.Where(q =>
                //    || q.ServiceName.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                //    || q.Label.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                //    || (q.Description.HasValue() && q.Description!.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                //    ).ToList();
            }

            rates = [.. rates
                .OrderBy(o => o.ProductName)
                .ThenBy(o => o.SkuName)
                .ThenBy(o => o.Type)
                .ThenBy(o => o.MeterName)
                .ThenBy(o => o.Location)
                .ThenBy(o => o.CurrencyCode)
                .ThenBy(o => o.TierMinimumUnits)
                ];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;

            var result = new ImportResult()
            {
                PlatformInfoId = request.PlatformInfoId,
                PlatformRegionId = request.PlatformRegionId,
                PlatformProductId = request.PlatformProductId,
                CurrencyCode = request.CurrencyCode,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Platforms = platforms,
                Regions = regions,
                Products = products,
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

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([command.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(command.PlatformInfoId);

            bool changes = false;
            foreach (var item in command.Items)
            {
                //var query = _dbcontext.Set<PlatformService>().FirstOrDefault(q => q.PlatformInfoId == platform.Id && q.Name == item.Name);
                //if (query is null)
                //{
                //    PlatformService record = new()
                //    {
                //        Id = Ulid.NewUlid(),
                //        PlatformInfo = platform,
                //        //Name = item.Name,
                //        //Label = item.Label,
                //        //Description = item.Description,
                //        //Remark = item.Remark,
                //        //CreatedDT = DateTime.UtcNow,

                //        //PlatformRegionId = item.PlatformRegionId,
                //        //PlatformProductId = item.PlatformProductId,
                //        //CostDriver = item.CostDriver,
                //        //CostBasedOn = item.CostBasedOn,
                //        //Limitations = item.Limitations,
                //        //AboutURL = item.AboutURL,
                //        //PricingURL = item.PricingURL,
                //        //ServiceId = item.ServiceId,
                //        //ServiceName = item.ServiceName,
                //        //Size = item.Size
                //    };
                //    _dbcontext.Set<PlatformService>().Add(record);
                //    changes = true;
                //}
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
