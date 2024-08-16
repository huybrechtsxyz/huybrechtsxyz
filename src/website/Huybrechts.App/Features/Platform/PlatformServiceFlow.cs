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
using System.ComponentModel.DataAnnotations.Schema;
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
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEmpty();
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
            RuleFor(m => m.Name).NotNull().Length(1, 128);
            RuleFor(m => m.Label).Length(1, 128);
            RuleFor(m => m.Category).Length(1, 128);
            RuleFor(m => m.Description).Length(1, 256);

            RuleFor(m => m.CostDriver).Length(1, 256);
            RuleFor(m => m.CostBasedOn).Length(1, 128);
            RuleFor(m => m.Limitations).Length(1, 512);
            RuleFor(m => m.AboutURL).Length(1, 512);
            RuleFor(m => m.PricingURL).Length(1, 512);
            RuleFor(m => m.PricingTier).Length(1, 128);
        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMSERVICE_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateRecordFound(string name) => Result.Fail(Messages.DUPLICATE_PLATFORMSERVICE_NAME.Replace("{0}", name.ToString()));

    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid platformInfoId, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();
        return await context.Set<PlatformRegion>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                         && pr.PlatformInfoId == platformInfoId
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

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

        public PlatformInfo Platform = new();
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
                    || (q.Category.HasValue() && q.Category!.Contains(searchString))
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

            var platform = await _dbcontext.Set<PlatformInfo>()
                .Where(q => q.Id == request.PlatformInfoId)
                .OrderBy(o => o.Name)
                .FirstOrDefaultAsync(cancellationToken: token);

            var model = new ListResult
            {
                PlatformInfoId = request.PlatformInfoId,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Platform = platform ?? new(),
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
        public CreateCommand Item { get; set; } = new();

        public PlatformInfo Platform { get; set; } = new();
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
            PlatformInfo? platform = await _dbcontext.Set<PlatformInfo>()
                .Where(q => q.Id == request.PlatformInfoId)
                .OrderBy(o => o.Name)
                .FirstOrDefaultAsync(token);
            if (platform is null)
                return PlatformNotFound(request.PlatformInfoId);

            return Result.Ok(new CreateResult()
            {
                Item = CreateNew(request.PlatformInfoId),
                Platform = platform
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

            if (await IsDuplicateNameAsync(_dbcontext, request.Name, request.PlatformInfoId))
                return DuplicateRecordFound(request.Name);

            var record = new PlatformService
            {
                Id = request.Id,
                PlatformInfo = platform,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Label = request.Label.Trim(),
                Category = request.Category.Trim(),
                Remark = request.Remark?.Trim(),
                CreatedDT = DateTime.UtcNow,

                CostBasedOn = request.CostBasedOn?.Trim(),
                CostDriver = request.CostDriver?.Trim(),
                Limitations = request.Limitations?.Trim(),
                AboutURL = request.AboutURL?.Trim(),
                PricingURL = request.PricingURL?.Trim(),
                PricingTier = request.PricingTier?.Trim(),
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
        public PlatformInfo Platform { get; set; } = new();
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

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(record.PlatformInfoId);

            record.Platform = platform;
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

            if (await IsDuplicateNameAsync(_dbcontext, command.Name, command.PlatformInfoId))
                return DuplicateRecordFound(command.Name);

            record.Name = command.Name.Trim();
            record.Description = command.Description?.Trim();
            record.Label = command.Label.Trim();
            record.Category = command.Category.Trim();
            record.Remark = command.Remark?.Trim();
            record.ModifiedDT = DateTime.UtcNow;

            record.CostBasedOn = command.CostBasedOn?.Trim();
            record.CostDriver = command.CostDriver?.Trim();
            record.Limitations = command.Limitations?.Trim();
            record.AboutURL = command.AboutURL?.Trim();
            record.PricingURL = command.PricingURL?.Trim();
            record.PricingTier = command.PricingTier?.Trim();

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
        public PlatformInfo Platform { get; set; } = new();
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
                .FirstOrDefaultAsync(token);
            if (record == null)
                return RecordNotFound(request.Id);

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == record.PlatformInfoId, cancellationToken: token);
            if (platform is null)
                return PlatformNotFound(record.PlatformInfoId);

            record.Platform = platform;
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

    public sealed record ImportModel : Model
    {
        [NotMapped]
        [Display(Name = nameof(IsSelected), ResourceType = typeof(Localization))]
        public bool IsSelected { get; set; }
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
        public PlatformInfo Platform = new();
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

        public ImportQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options)
            : base(dbcontext, configuration)
        {
            _options = options;
        }

        public async Task<ImportResult> Handle(ImportQuery request, CancellationToken token)
        {
            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(f => f.Id == request.PlatformInfoId, cancellationToken: token);
            if (platform is null)
                return new ImportResult()
                {
                    Platform = new(),
                    CurrentFilter = request.CurrentFilter,
                    SearchText = request.SearchText,
                    SortOrder = request.SortOrder,
                    Results = []
                };

            var searchString = request.SearchText ?? request.CurrentFilter;
            List<ImportModel> Services = await GetAzureServicesAsync(platform.Provider.ToString(), request.PlatformInfoId, searchString);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                Services = Services.Where(q =>
                    q.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                    || q.Label.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                    || (q.Category.HasValue() && q.Category!.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
                    ).ToList();
            }

            Services = [.. Services.OrderBy(o => o.Name)];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = request.Page ?? 1;

            return new ImportResult()
            {
                Platform = platform,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = request.SortOrder,
                Results = new PaginatedList<ImportModel>(
                    Services.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                    Services.Count,
                    pageNumber,
                    pageSize)
            };
        }

        private async Task<List<ImportModel>> GetAzureServicesAsync(string platform, Ulid platformInfoId, string searchString)
        {
            List<ImportModel> result = [];

            var service = new AzurePricingService(_options);
            var pricing = await service.GetServicesAsync("", "", "", searchString);

            if (pricing is null)
                return [];

            foreach (var item in pricing.Items ?? [])
            {
                if (item is null || string.IsNullOrEmpty(item.ServiceName))
                    continue;

                result.Add(new ImportModel()
                {
                    Id = Ulid.NewUlid(),
                    PlatformInfoId = platformInfoId,
                    Name = item.ServiceName,
                    Label = item.ServiceName,
                    Category = item.ServiceFamily ?? string.Empty,
                    //CostBasedOn = ,
                    //CostDriver = ,
                    //Limitations = ,
                    //AboutURL =
                    //PricingURL = ,
                    //PricingTier = item.SkuName
                }); ;
            }

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
                var query = await _dbcontext.Set<PlatformService>().FirstOrDefaultAsync(q => q.PlatformInfoId == platform.Id && q.Name == item.Name, cancellationToken: token);
                if (query is null)
                {
                    PlatformService record = new()
                    {
                        Id = Ulid.NewUlid(),
                        PlatformInfo = platform,
                        Name = item.Name,
                        Label = item.Label,
                        Category = item.Category,
                        Description = item.Description,
                        Remark = item.Remark,
                        CreatedDT = DateTime.UtcNow,

                        CostBasedOn = item.CostBasedOn,
                        CostDriver = item.CostDriver,
                        Limitations = item.Limitations,
                        AboutURL = item.AboutURL,
                        PricingURL = item.PricingURL,
                        PricingTier = item.PricingTier
                    };
                    await _dbcontext.Set<PlatformService>().AddAsync(record, token);
                    changes = true;
                }
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
