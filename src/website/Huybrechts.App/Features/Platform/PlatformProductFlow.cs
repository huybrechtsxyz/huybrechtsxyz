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
        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMPRODUCT_ID.Replace("{0}", id.ToString()));

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

    public sealed class ListQuery : EntityListFlow.Query, IRequest<ListResult>
    {
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;
    }

    internal sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
        public PlatformInfo Platform = new();
    }

    internal sealed class ListHandler :
        EntityListFlow.Handler<PlatformProduct, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<ListResult> Handle(ListQuery message, CancellationToken token)
        {
            IQueryable<PlatformProduct> query = _dbcontext.Set<PlatformProduct>();

            if (message.PlatformInfoId.HasValue)
            {
                query = query.Where(q => q.PlatformInfoId == message.PlatformInfoId);
            }

            var searchString = message.SearchText ?? message.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(q =>
                    q.Name.Contains(searchString)
                    || (q.Category.HasValue() && q.Category!.Contains(searchString))
                    || (q.Description.HasValue() && q.Description!.Contains(searchString)));
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

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == message.PlatformInfoId, cancellationToken: token);

            var model = new ListResult
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = message.SortOrder,
                Platform = platform ?? new(),
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

    internal sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
    {
        public CreateQueryValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
    {
        public PlatformInfo Platform { get; set; } = new();
    }

    internal sealed class CreateCommandValidator : ModelValidator<CreateCommand>
    {
        public CreateCommandValidator() : base()
        {
        }
    }

    internal class CreateQueryHandler : IRequestHandler<CreateQuery, Result<CreateCommand>>
    {
        private readonly PlatformContext _dbcontext;

        public CreateQueryHandler(PlatformContext dbcontext)
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
        private readonly PlatformContext _dbcontext;

        public CreateCommandHandler(PlatformContext dbcontext)
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
                Category = message.Category.Trim(),
                Remark = message.Remark?.Trim(),
                CreatedDT = DateTime.UtcNow
            };

            await _dbcontext.Set<PlatformProduct>().AddAsync(record, token);
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

    internal class UpdateCommandValidator : ModelValidator<UpdateCommand> { }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformProduct, UpdateCommand>()
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

        public async Task<Result> Handle(UpdateCommand message, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformProduct>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, message.PlatformInfoId))
                return DuplicateRecordFound(message.Name);

            record.Name = message.Name.Trim();
            record.Description = message.Description?.Trim();
            record.Label = message.Label.Trim();
            record.Category = message.Category.Trim();
            record.Remark = message.Remark?.Trim();
            record.ModifiedDT = DateTime.UtcNow;

            _dbcontext.Set<PlatformProduct>().Update(record);
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
            CreateProjection<PlatformProduct, DeleteCommand>()
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

    internal sealed class ImportQueryValidator : AbstractValidator<ImportQuery>
    {
        public ImportQueryValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotNull().NotEmpty();
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

    internal sealed class ImportCommandValidator : AbstractValidator<ImportCommand>
    {
        public ImportCommandValidator()
        {
            RuleFor(m => m.PlatformInfoId).NotNull();
        }
    }

    internal sealed class ImportQueryHandler :
       EntityListFlow.Handler<PlatformProduct, ImportModel>,
       IRequestHandler<ImportQuery, Result<ImportResult>>
    {
        private readonly PlatformImportOptions _options;

        public ImportQueryHandler(PlatformContext dbcontext, IConfigurationProvider configuration, PlatformImportOptions options)
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
            List<ImportModel> Services = await GetAzureServicesAsync(platform.Provider.ToString(), message.PlatformInfoId, searchString);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                Services = Services.Where(q =>
                    q.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                    || q.Label.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                    ).ToList();
            }

            Services = [.. Services.OrderBy(o => o.Name)];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;

            return new ImportResult()
            {
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = message.SortOrder,
                Platform = platform,
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
            var pricing = await service.GetProductsAsync("", "", "", searchString);

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
                    Category = item.ServiceFamily ?? string.Empty
                });
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
                        Name = item.Name,
                        Label = item.Label,
                        Category = item.Category,
                        Description = item.Description,
                        Remark = item.Remark,
                        CreatedDT = DateTime.UtcNow
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
