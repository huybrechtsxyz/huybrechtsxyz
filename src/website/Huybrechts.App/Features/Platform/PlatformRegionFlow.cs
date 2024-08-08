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
using Newtonsoft.Json;
using StackExchange.Profiling.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing.Printing;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text.Json.Serialization;
using static Huybrechts.App.Services.AzurePricingService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.NOT_FOUND_PLATFORMREGION_ID.Replace("{0}", id.ToString()));

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
        public Ulid? PlatformInfoId { get; set; } = Ulid.Empty;

        public IList<PlatformInfo>? Platforms = null;
    }

    internal sealed class ImportQueryHandler :
       EntityListFlow.Handler<PlatformRegion, ImportModel>,
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

            List<ImportModel> regions = await GetAzureLocationsAsync(platform.Provider.ToString(), request.PlatformInfoId);

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

        private async Task<List<ImportModel>> GetAzureLocationsAsync(string platform, Ulid platformInfoId)
        {
            List<ImportModel> result = [];
            var service = new AzurePricingService(_options);
            var pricing = await service.GetRegionsAsync();

            if (pricing is null)
                return [];

            foreach (var item in pricing.Items ?? [])
            {
                if (item is null || string.IsNullOrEmpty(item.Location))
                    continue;

                result.Add(new ImportModel()
                {
                    Id = Ulid.NewUlid(),
                    PlatformInfoId = platformInfoId,
                    Name = item.Location,
                    Label = item.ArmRegionName ?? item.Location
                });
            }

            return result;
        }
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
                var query = _dbcontext.Set<PlatformRegion>().FirstOrDefault(q => q.PlatformInfoId == platform.Id && q.Name == item.Name);
                if (query is null)
                {
                    PlatformRegion record = new()
                    {
                        Id = Ulid.NewUlid(),
                        PlatformInfo = platform,
                        Name = item.Name,
                        Label = item.Label,
                        Description = item.Description,
                        Remark = item.Remark,
                        CreatedDT = DateTime.UtcNow
                    };
                    _dbcontext.Set<PlatformRegion>().Add(record);
                    changes = true;
                }
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }

}
