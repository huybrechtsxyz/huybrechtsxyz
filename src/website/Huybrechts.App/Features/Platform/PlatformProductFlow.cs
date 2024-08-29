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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Platform;

public static class PlatformProductFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = "Platform", ResourceType = typeof(Localization))]
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        [Display(Name = "Platform", ResourceType = typeof(Localization))]
        public string PlatformInfoName { get; set; } = string.Empty;

        [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Label), ResourceType = typeof(Localization))]
        public string Label { get; set; } = string.Empty;
        
        [Display(Name = nameof(Category), ResourceType = typeof(Localization))]
        public string Category { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(CostDriver), ResourceType = typeof(Localization))]
        public string? CostDriver { get; set; }

        [Display(Name = nameof(CostBasedOn), ResourceType = typeof(Localization))]
        public string? CostBasedOn { get; set; }

        [Display(Name = nameof(Limitations), ResourceType = typeof(Localization))]
        public string? Limitations { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = nameof(AboutURL), ResourceType = typeof(Localization))]
        public string? AboutURL { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = nameof(PricingURL), ResourceType = typeof(Localization))]
        public string? PricingURL { get; set; }

        [Display(Name = nameof(PricingTier), ResourceType = typeof(Localization))]
        public string? PricingTier { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }

        public string SearchIndex => $"{Name}~{Label}~{Category}~{Description}".ToLowerInvariant();
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEmpty();
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
            RuleFor(m => m.Name).NotEmpty().Length(1, 128);
            RuleFor(m => m.Label).NotEmpty().Length(1, 128);
            RuleFor(m => m.Category).Length(0, 128);
            RuleFor(m => m.Description).Length(0, 256);

            RuleFor(m => m.CostDriver).Length(0, 256);
            RuleFor(m => m.CostBasedOn).Length(0, 128);
            RuleFor(m => m.Limitations).Length(0, 512);
            RuleFor(m => m.AboutURL).Length(0, 512);
            RuleFor(m => m.PricingURL).Length(0, 512);
            RuleFor(m => m.PricingTier).Length(0, 128);
        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMPRODUCT_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateRecordFound(string name) => Result.Fail(Messages.DUPLICATE_PLATFORMPRODUCT_NAME.Replace("{0}", name.ToString()));

    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid platformInfoId, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();
        return await context.Set<PlatformProduct>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                         && pr.PlatformInfoId == platformInfoId
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

    //
    // LIST
    //

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformProduct, ListModel>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
    }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>>
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

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public PlatformInfo Platform = new();
    }

    internal sealed class ListHandler :
        EntityListFlow.Handler<PlatformProduct, ListModel>,
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
                return PlatformNotFound(message.PlatformInfoId ?? Ulid.Empty);

            IQueryable<PlatformProduct> query = _dbcontext.Set<PlatformProduct>();

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
            else query = query.OrderBy(o => o.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;
            var results = await query
                .Include(i => i.PlatformInfo)
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new ListResult
            {
                PlatformInfoId = message.PlatformInfoId,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = message.SortOrder,
                Platform = platform,
                Results = results ?? []
            };

            return model;
        }
    }

    //
    // CREATE
    //

    public static CreateCommand CreateNew(PlatformInfo platform) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = platform.Id,
        PlatformInfoName = platform.Name
    };

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

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

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

            RuleFor(x => x).MustAsync(async (model, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, model.Name, model.PlatformInfoId);
                return !exists;
            })
            .WithMessage(x => Messages.DUPLICATE_PLATFORMPRODUCT_NAME.Replace("{0}", x.Name.ToString()))
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
            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([message.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(message.PlatformInfoId);

            var record = CreateNew(platform);

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
                return PlatformNotFound(message.PlatformInfoId);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, message.PlatformInfoId))
                return DuplicateRecordFound(message.Name);

            var record = new PlatformProduct
            {
                Id = message.Id,
                PlatformInfo = platform,
                Name = message.Name.Trim(),
                Description = message.Description?.Trim(),
                Label = message.Label.Trim(),
                Category = message.Category?.Trim(),
                Remark = message.Remark?.Trim(),
                SearchIndex = message.SearchIndex,
                CreatedDT = DateTime.UtcNow,

                CostBasedOn = message.CostBasedOn?.Trim(),
                CostDriver = message.CostDriver?.Trim(),
                Limitations = message.Limitations?.Trim(),
                AboutURL = message.AboutURL?.Trim(),
                PricingURL = message.PricingURL?.Trim(),
                PricingTier = message.PricingTier?.Trim(),
            };

            await _dbcontext.Set<PlatformProduct>().AddAsync(record, token);
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
            RuleFor(m => m.Id).NotNull().NotEqual(Ulid.Empty);
        }
    }

    public record UpdateCommand : Model, IRequest<Result> { }

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

            RuleFor(x => x).MustAsync(async (model, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, model.Name, model.PlatformInfoId, model.Id);
                return !exists;
            })
            .WithMessage(x => Messages.DUPLICATE_PLATFORMPRODUCT_NAME.Replace("{0}", x.Name.ToString()))
            .WithName(nameof(CreateCommand.Name));
        }
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformProduct, UpdateCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
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
            var record = await _dbcontext.Set<PlatformProduct>()
                .Include(i => i.PlatformInfo)
                .ProjectTo<UpdateCommand>(_configuration)
                .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

            if (record == null) 
                return RecordNotFound(message.Id);

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([record.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(record.PlatformInfoId);
            //else
            //    record.Platform = platform;

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
            var record = await _dbcontext.Set<PlatformProduct>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, message.PlatformInfoId, record.Id))
                return DuplicateRecordFound(message.Name);

            record.Name = message.Name.Trim();
            record.Description = message.Description?.Trim();
            record.Label = message.Label.Trim();
            record.Category = message.Category?.Trim();
            record.Remark = message.Remark?.Trim();
            record.SearchIndex = message.SearchIndex;
            record.ModifiedDT = DateTime.UtcNow;

            record.CostBasedOn = message.CostBasedOn?.Trim();
            record.CostDriver = message.CostDriver?.Trim();
            record.Limitations = message.Limitations?.Trim();
            record.AboutURL = message.AboutURL?.Trim();
            record.PricingURL = message.PricingURL?.Trim();
            record.PricingTier = message.PricingTier?.Trim();

            _dbcontext.Set<PlatformProduct>().Update(record);
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
        public DeleteCommandMapping() => 
            CreateProjection<PlatformProduct, DeleteCommand>()
            .ForMember(dest => dest.PlatformInfoName, opt => opt.MapFrom(src => src.PlatformInfo.Name));
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
            var record = await _dbcontext.Set<PlatformProduct>()
                .Include(i => i.PlatformInfo)
                .ProjectTo<DeleteCommand>(_configuration)
                .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

            if (record == null)
                return RecordNotFound(message.Id);

            var platform = await _dbcontext.Set<PlatformInfo>().FindAsync([record.PlatformInfoId], cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(record.PlatformInfoId);
            //else
            //    record.Platform = platform;

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
            var record = await _dbcontext.Set<PlatformProduct>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            _dbcontext.Set<PlatformProduct>().Remove(record);
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

    public sealed class ImportQuery : EntityListFlow.Query, IRequest<Result<ImportResult>>
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

    public sealed class ImportResult : EntityListFlow.Result<ImportModel>
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
       EntityListFlow.Handler<PlatformProduct, ImportModel>,
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
                return PlatformNotFound(message.PlatformInfoId);

            var searchString = message.SearchText ?? message.CurrentFilter;
            List<ImportModel> records = await GetAzureProductsAsync(platform.Provider.ToString(), message.PlatformInfoId, searchString);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchFor = searchString.ToLowerInvariant();
                records = records.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
            }

            records = [.. records.OrderBy(o => o.Name)];
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

        private async Task<List<ImportModel>> GetAzureProductsAsync(string platform, Ulid platformInfoId, string searchString)
        {
            List<ImportModel> result = [];
            
            var service = new AzurePricingService(_options);
            var pricing = await service.GetProductsAsync("", "", "", searchString);

            if (pricing is null)
                return [];

            foreach (var item in pricing.Items ?? [])
            {
                if (item is null || string.IsNullOrEmpty(item.ProductName))
                    continue;

                result.Add(new ImportModel()
                {
                    Id = Ulid.NewUlid(),
                    PlatformInfoId = platformInfoId,
                    Name = item.ProductName,
                    Label = item.ProductName,
                    Category = item.ServiceFamily ?? string.Empty,
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
                return PlatformNotFound(message.PlatformInfoId);

            bool changes = false;
            foreach (var item in message.Items)
            {
                var query = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(q => q.PlatformInfoId == platform.Id && q.Name == item.Name, cancellationToken: token);
                if (query is null)
                {
                    PlatformProduct record = new()
                    {
                        Id = Ulid.NewUlid(),
                        PlatformInfo = platform,
                        Name = item.Name.Trim(),
                        Label = item.Label.Trim(),
                        Category = item.Category.Trim(),
                        Description = item.Description?.Trim(),
                        Remark = item.Remark?.Trim(),
                        CreatedDT = DateTime.UtcNow,

                        CostBasedOn = item.CostBasedOn,
                        CostDriver = item.CostDriver,
                        Limitations = item.Limitations,
                        AboutURL = item.AboutURL,
                        PricingURL = item.PricingURL,
                        PricingTier = item.PricingTier
                    };
                    await _dbcontext.Set<PlatformProduct>().AddAsync(record, token);
                    changes = true;
                }
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
