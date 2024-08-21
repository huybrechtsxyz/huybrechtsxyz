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

public static class PlatformRegionFlow
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

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
        public string? Remark { get; set; }

        public string SearchIndex => $"{Name}~{Label}~{Description}".ToLowerInvariant();
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

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMREGION_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateRecordFound(string name) => Result.Fail(Messages.DUPLICATE_PLATFORMREGION_NAME.Replace("{0}", name.ToString()));

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

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformRegion, ListModel>()
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
        EntityListFlow.Handler<PlatformRegion, ListModel>,
        IRequestHandler<ListQuery, Result<ListResult>>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
        {
            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == message.PlatformInfoId, cancellationToken: token);
            if (platform == null)
                return PlatformNotFound(message.PlatformInfoId ?? Ulid.Empty);

            IQueryable<PlatformRegion> query = _dbcontext.Set<PlatformRegion>();

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
        //Platform = platform,
        PlatformInfoId = platform.Id,
        PlatformInfoName = platform.Name,
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

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
    {
        //public PlatformInfo Platform { get; set; } = new();
    }

    public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
    {
        public CreateCommandValidator(PlatformContext dbContext) : base()
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
            .WithMessage(x => Messages.DUPLICATE_PLATFORM_NAME.Replace("{0}", x.Name.ToString()))
            .WithName(nameof(CreateCommand.Name));
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

            var record = new PlatformRegion
            {
                Id = message.Id,
                PlatformInfo = platform,
                Name = message.Name.Trim(),
                Description = message.Description?.Trim(),
                Label = message.Label.Trim(),
                Remark = message.Remark?.Trim(),
                SearchIndex = message.SearchIndex,
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

    public sealed record UpdateQuery : IRequest<Result<UpdateCommand>> { public Ulid Id { get; init; } }

    public sealed class UpdateQueryValidator : AbstractValidator<UpdateQuery>
    {
        public UpdateQueryValidator()
        {
            RuleFor(m => m.Id).NotNull().NotEqual(Ulid.Empty);
        }
    }

    public record UpdateCommand : Model, IRequest<Result>
    {
        //public PlatformInfo Platform { get; set; } = new();
    }

    public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
    {
        public UpdateCommandValidator(PlatformContext dbContext)
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
            .WithMessage(x => Messages.DUPLICATE_PLATFORM_NAME.Replace("{0}", x.Name.ToString()))
            .WithName(nameof(CreateCommand.Name));
        }
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformRegion, UpdateCommand>()
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
            var record = await _dbcontext.Set<PlatformRegion>()
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
        private readonly PlatformContext _dbcontext;

        public UpdateCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(UpdateCommand message, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRegion>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, message.PlatformInfoId, record.Id))
                return DuplicateRecordFound(message.Name);

            record.Name = message.Name.Trim();
            record.Description = message.Description?.Trim();
            record.Label = message.Label.Trim();
            record.Remark = message.Remark?.Trim();
            record.SearchIndex = message.SearchIndex;
            record.ModifiedDT = DateTime.UtcNow;

            _dbcontext.Set<PlatformRegion>().Update(record);
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

    public sealed record DeleteCommand : Model, IRequest<Result>
    {
        //public PlatformInfo Platform { get; set; } = new();
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
            CreateProjection<PlatformRegion, DeleteCommand>()
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
            var record = await _dbcontext.Set<PlatformRegion>()
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
        private readonly PlatformContext _dbcontext;

        public DeleteCommandHandler(PlatformContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result> Handle(DeleteCommand message, CancellationToken token)
        {
            var record = await _dbcontext.Set<PlatformRegion>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

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
       EntityListFlow.Handler<PlatformRegion, ImportModel>,
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
            List<ImportModel> regions = await GetAzureLocationsAsync(platform.Provider.ToString(), message.PlatformInfoId, searchString);
            
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchFor = searchString.ToLowerInvariant();
                regions = regions.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor)).ToList();
            }

            regions = [.. regions.OrderBy(o => o.Name)];
            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;

            return new ImportResult()
            {
                Platform = platform,
                CurrentFilter = searchString,
                SearchText = searchString,
                SortOrder = message.SortOrder,
                Results = new PaginatedList<ImportModel>(
                    regions.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                    regions.Count,
                    pageNumber,
                    pageSize)
            };
        }

        private async Task<List<ImportModel>> GetAzureLocationsAsync(string platform, Ulid platformInfoId, string searchString)
        {
            List<ImportModel> result = [];
            
            var service = new AzurePricingService(_options);
            var pricing = await service.GetRegionsAsync("", "", "", searchString);

            if (pricing is null)
                return [];

            foreach (var item in pricing.Items ?? [])
            {
                if (item is null || string.IsNullOrEmpty(item.ArmRegionName))
                    continue;

                result.Add(new ImportModel()
                {
                    Id = Ulid.NewUlid(),
                    PlatformInfoId = platformInfoId,
                    Name = item.ArmRegionName,
                    Label = item.Location ?? item.ArmRegionName
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
                var query = await _dbcontext.Set<PlatformRegion>().FirstOrDefaultAsync(q => q.PlatformInfoId == platform.Id && q.Name == item.Name, cancellationToken: token);
                if (query is null)
                {
                    PlatformRegion record = new()
                    {
                        Id = Ulid.NewUlid(),
                        PlatformInfo = platform,
                        Name = item.Name.Trim(),
                        Label = item.Label.Trim(),
                        Description = item.Description?.Trim(),
                        Remark = item.Remark?.Trim(),
                        CreatedDT = DateTime.UtcNow
                    };
                    await _dbcontext.Set<PlatformRegion>().AddAsync(record, token);
                    changes = true;
                }
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
