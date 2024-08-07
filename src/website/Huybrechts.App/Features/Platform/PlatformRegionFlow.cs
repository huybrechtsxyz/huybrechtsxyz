using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Profiling.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq.Dynamic.Core;
using System.Text.Json.Serialization;

namespace Huybrechts.App.Features.Platform;

public static class PlatformRegionFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = "Platform", ResourceType = typeof(PlatformLocalization))]
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        [Display(Name = "Platform", ResourceType = typeof(PlatformLocalization))]
        public string PlatformInfoName { get; set; } = string.Empty;

        [Display(Name = nameof(Name), ResourceType = typeof(PlatformLocalization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Label), ResourceType = typeof(PlatformLocalization))]
        public string Label { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(PlatformLocalization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(PlatformLocalization))]
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
        }
    }

    private static Result RecordNotFound(Ulid id) => Result.Fail($"Unable to find platform region with ID {id}");

    //
    // LIST
    //

    public sealed record ListModel : Model
    {
    }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformRegion, ListModel>()
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
        EntityListFlow.Handler<PlatformRegion, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<ListResult> Handle(ListQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlatformRegion> query = _dbcontext.Set<PlatformRegion>();

            if (request.PlatformInfoId.HasValue)
            {
                query.Where(q => q.PlatformInfoId == request.PlatformInfoId);
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

    public sealed record CreateQuery : IRequest<CreateResult>
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;
    }

    internal sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
    {
        public CreateQueryValidator()
        {
        }
    }

    public record CreateResult
    {
        public CreateCommand Region { get; set; } = new();

        public IList<PlatformInfo> Platforms { get; set; } = [];
    }

    internal class CreateQueryHandler : IRequestHandler<CreateQuery, CreateResult>
    {
        private readonly PlatformContext _dbcontext;

        public CreateQueryHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<CreateResult> Handle(CreateQuery request, CancellationToken token)
        {
            IList<PlatformInfo> platforms = await _dbcontext.Set<PlatformInfo>().ToListAsync(cancellationToken: token);

            return new CreateResult()
            {
                Region = CreateNew(request.PlatformInfoId),
                Platforms = platforms
            };
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
            var record = new PlatformRegion
            {
                Id = request.Id,
                PlatformInfoId = request.PlatformInfoId,
                Name = request.Name,
                Description = request.Description,
                Label = request.Label,
                Remark = request.Remark,
                CreatedDT = DateTime.UtcNow
            };
            await _dbcontext.Set<PlatformRegion>().AddAsync(record, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(record.Id);
        }
    }

    //
    // UPDATE
    //

    public sealed record UpdateQuery : IRequest<UpdateCommand?>
    {
        public Ulid? Id { get; init; }
    }

    internal sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
    {
        public UpdateQueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record UpdateCommand : Model, IRequest<Result>
    {
    }

    internal class UpdateCommandValidator : ModelValidator<UpdateCommand>
    {
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformRegion, UpdateCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
    }

    internal class UpdateQueryHandler : IRequestHandler<UpdateQuery, UpdateCommand?>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public UpdateQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<UpdateCommand?> Handle(UpdateQuery request, CancellationToken token)
        {
            return await _dbcontext.Set<PlatformRegion>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformInfo)
                .ProjectTo<UpdateCommand>(_configuration)
                .SingleOrDefaultAsync(token);
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
            var record = await _dbcontext.Set<PlatformRegion>().FindAsync([command.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(command.Id);

            record.Name = command.Name;
            record.Description = command.Description;
            record.Label = command.Label;
            record.Remark = command.Remark;
            record.ModifiedDT = DateTime.UtcNow;

            _dbcontext.Set<PlatformRegion>().Update(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // DELETE
    //

    public sealed record DeleteQuery : IRequest<DeleteCommand>
    {
        public Ulid? Id { get; init; }
    }

    internal class DeleteQueryValidator : AbstractValidator<DeleteQuery>
    {
        public DeleteQueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public sealed record DeleteCommand : Model, IRequest<Result>
    {
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
            CreateProjection<PlatformRegion, DeleteCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
    }

    internal sealed class DeleteQueryHandler : IRequestHandler<DeleteQuery, DeleteCommand?>
    {
        private readonly PlatformContext _dbcontext;
        private readonly IConfigurationProvider _configuration;

        public DeleteQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<DeleteCommand?> Handle(DeleteQuery request, CancellationToken token)
        {
            return await _dbcontext.Set<PlatformRegion>()
                .Where(s => s.Id == request.Id)
                .Include(i => i.PlatformInfo)
                .ProjectTo<DeleteCommand>(_configuration)
                .SingleOrDefaultAsync(token);
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
            var record = await _dbcontext.Set<PlatformRegion>().FindAsync([command.Id], cancellationToken: token);
            if(record is null)
                return RecordNotFound(command.Id);
            _dbcontext.Set<PlatformRegion>().Remove(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // IMPORT
    //

    public sealed record ImportModel : Model
    {

    }

    public sealed class ImportQuery : EntityListFlow.Query, IRequest<ImportResult>
    {
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;
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

        public IList<PlatformInfo>? Platforms = null;
    }

    internal sealed class ImportHandler :
       EntityListFlow.Handler<PlatformRegion, ImportModel>,
       IRequestHandler<ImportQuery, ImportResult>
    {
        private readonly PlatformImportOptions _options;

        public ImportHandler(PlatformContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options)
            : base(dbcontext, configuration)
        {
            _options = options;
        }

        public async Task<ImportResult> Handle(ImportQuery request, CancellationToken cancellationToken)
        {
            var platforms = _dbcontext.Set<PlatformInfo>().OrderBy(o => o.Name).ToList();

            List<ImportModel> regions = await GetAzureLocationsAsync(request.PlatformInfoId);

            var searchString = request.SearchText ?? request.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                regions = regions.Where(q =>
                    q.Name.Contains(searchString)
                    || q.Label.Contains(searchString)
                    || (q.Description.HasValue() && q.Description!.Contains(searchString))
                    ).ToList();
            }

            regions = [.. regions.OrderBy(o => o.Name)];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;

            return new ImportResult()
            {
                PlatformInfoId = request.PlatformInfoId,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Platforms = platforms,
                Results = new PaginatedList<ImportModel>(
                    regions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                    regions.Count,
                    pageNumber,
                    pageSize)
            };
        }

        private async Task<List<ImportModel>> GetAzureLocationsAsync(Ulid platformInfoId)
        {
            var azureOptions = _options.Platforms["Azure"];
            var locations = new HashSet<string>();
            string requestUrl = azureOptions.Regions;
            int maxPages = 20;
            int currentPage = 0;

            List<ImportModel> result = [];
            using HttpClient httpClient = new();

            while (!string.IsNullOrEmpty(requestUrl) && currentPage < maxPages)
            {
                try
                {
                    var response = await httpClient.GetStringAsync(requestUrl);
                    var pricingResponse = JsonConvert.DeserializeObject<PricingResponse>(response);
                    if (pricingResponse is not null)
                    { 
                        foreach (var item in pricingResponse.Items ?? [])
                        {
                            if (!string.IsNullOrEmpty(item.ArmRegionName))
                            {
                                if (locations.Add(item.ArmRegionName))
                                {
                                    result.Add(new ImportModel()
                                    {
                                        Id = Ulid.NewUlid(),
                                        PlatformInfoId = platformInfoId,
                                        Name = item.ArmRegionName,
                                        Label = item.Location ?? item.ArmRegionName
                                    });
                                }
                            }
                        }
                    }

                    requestUrl = pricingResponse?.NextPageLink ?? string.Empty;
                    currentPage++;
                }
                catch (HttpRequestException e)
                {
                    // Handle the exception (e.g., log the error)
                    break; // Exit the loop if there's an issue with the request
                }
            }

            return result;
        }
    }

    private class PricingResponse
    {
        public string? BillingCurrency { get; set; }

        public string? CustomerEntityId { get; set; }

        public string? CustomerEntityType { get; set; }

        public List<PricingItem>? Items { get; set; }

        public string? NextPageLink { get; set; }

        public int Count { get; set; }
    }

    public class PricingItem
    {
        /// <summary>
        /// "currencyCode":"USD"
        /// </summary>
        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// "tierMinimumUnits":0.0
        /// </summary>
        [JsonPropertyName("tierMinimumUnits")]
        public double TierMinimumUnits { get; set; }

        /// <summary>
        /// "retailPrice":0.29601
        /// </summary>
        [JsonPropertyName("retailPrice")]
        public double RetailPrice { get; set; }

        /// <summary>
        /// "unitPrice":0.29601
        /// </summary>
        [JsonPropertyName("unitPrice")]
        public double UnitPrice { get; set; }

        /// <summary>
        /// "armRegionName":"southindia"
        /// </summary>
        [JsonPropertyName("armRegionName")]
        public string? ArmRegionName { get; set; }

        /// <summary>
        /// "location":"IN South"
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// "":"2024-08-01T00:00:00Z"
        /// </summary>
        [JsonPropertyName("effectiveStartDate")]
        public string? EffectiveStartDate { get; set; }

        /// <summary>
        /// "meterId":"000009d0-057f-5f2b-b7e9-9e26add324a8"
        /// </summary>
        [JsonPropertyName("meterId")]
        public string? MeterId { get; set; }

        /// <summary>
        /// "meterName":"D14/DS14 Spot"
        /// </summary>
        [JsonPropertyName("meterName")]
        public string? MeterName { get; set; }

        /// <summary>
        /// "productId":"DZH318Z0BPVW"
        /// </summary>
        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }

        /// <summary>
        /// "skuId":"DZH318Z0BPVW/00QZ"
        /// </summary>
        [JsonPropertyName("skuId")]
        public string? SkuId { get; set; }

        /// <summary>
        /// "productName":"Virtual Machines D Series Windows"
        /// </summary>
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }

        /// <summary>
        /// "skuName":"D14 Spot"
        /// </summary>
        [JsonPropertyName("skuName")]
        public string? SkuName { get; set; }

        /// <summary>
        /// "serviceName":"Virtual Machines"
        /// </summary>
        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        /// <summary>
        /// "serviceId":"DZH313Z7MMC8"
        /// </summary>
        [JsonPropertyName("serviceId")]
        public string? ServiceId { get; set; }

        /// <summary>
        /// "serviceFamily":"Compute"
        /// </summary>
        [JsonPropertyName("serviceFamily")]
        public string? ServiceFamily { get; set; }

        /// <summary>
        /// "unitOfMeasure":"1 Hour"
        /// </summary>
        [JsonPropertyName("unitOfMeasure")]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// "type":"Consumption"
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// "isPrimaryMeterRegion": true
        /// </summary>
        [JsonPropertyName("isPrimaryMeterRegion")]
        public bool IsPrimaryMeterRegion { get; set; }

        /// <summary>
        /// "armSkuName":"Standard_D14"
        /// </summary>
        [JsonPropertyName("armSkuName")]
        public string? ArmSkuName { get; set; }
    }

}
