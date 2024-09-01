using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Platform;

public static class PlatformRateUnitFlow
{
    public static async Task<List<SetupUnit>> GetSetupUnitsAsync(FeatureContext dbcontext, CancellationToken token)
    {
        return await dbcontext.Set<SetupUnit>()
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken: token);
    }


    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = "Platform", ResourceType = typeof(Localization))]
        public Ulid PlatformInfoId { get; set; } = Ulid.Empty;

        [Display(Name = "Product", ResourceType = typeof(Localization))]
        public Ulid PlatformProductId { get; set; } = Ulid.Empty;

        [Display(Name = "Rate", ResourceType = typeof(Localization))]
        public Ulid PlatformRateId { get; init; } = Ulid.Empty;

        [Display(Name = "Rate", ResourceType = typeof(Localization))]
        public PlatformRate PlatformRate { get; set; } = new();

        [Display(Name = "SetupUnit", ResourceType = typeof(Localization))]
        public Ulid SetupUnitId { get; init; } = Ulid.Empty;

        [Display(Name = "SetupUnit", ResourceType = typeof(Localization))]
        public SetupUnit SetupUnit { get; init; } = new();

        [Precision(12, 6)]
        [Display(Name = nameof(UnitFactor), ResourceType = typeof(Localization))]
        public decimal UnitFactor { get; set; } = 0;

        [Precision(12, 4)]
        [Display(Name = nameof(DefaultValue), ResourceType = typeof(Localization))]
        public decimal DefaultValue { get; set; } = 0;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string Description { get; set; } = string.Empty;

        public string SearchIndex => $"{PlatformRate.UnitOfMeasure}~{SetupUnit.Name}~{Description}".ToLowerInvariant();
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.PlatformInfoId).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.PlatformProductId).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.PlatformRateId).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.Description).NotEmpty().Length(1, 128);

            RuleFor(m => m.UnitFactor).NotNull();
            RuleFor(m => m.DefaultValue).NotNull();
        }
    }

    private static Result PlatformNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    private static Result ProductNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMPRODUCT_ID.Replace("{0}", id.ToString()));

    private static Result RateNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMRATE_ID.Replace("{0}", id.ToString()));

    private static Result UnitNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPUNIT_ID.Replace("{0}", id.ToString()));

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORMRATEUNIT_ID.Replace("{0}", id.ToString()));

    //
    // LIST
    //

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile
    {
        public ListMapping() =>
            CreateProjection<PlatformRateUnit, ListModel>();
    }

    public sealed record ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>>
    {
        public Ulid? PlatformRateId { get; set; } = Ulid.Empty;
    }

    public sealed class ListValidator : AbstractValidator<ListQuery> 
    {
        public ListValidator() 
        { 
            RuleFor(x => x.PlatformRateId).NotEmpty().NotEqual(Ulid.Empty); 
        } 
    }

    public sealed record ListResult : EntityListFlow.Result<ListModel>
    {
        public Ulid? PlatformRateId { get; set; } = Ulid.Empty;

        public PlatformInfo Platform { get; set; } = new();

        public PlatformProduct Product { get; set; } = new();

        public PlatformRate Rate { get; set; } = new();
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
            var rate = await _dbcontext.Set<PlatformRate>().FirstOrDefaultAsync(q => q.Id == message.PlatformRateId, cancellationToken: token);
            if (rate == null)
                return RateNotFound(message.PlatformRateId ?? Ulid.Empty);

            var product = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(q => q.Id == rate.PlatformProductId, cancellationToken: token);
            if (product == null)
                return ProductNotFound(rate.PlatformProductId);

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == rate.PlatformInfoId, cancellationToken: token);
            if (platform == null)
                return PlatformNotFound(rate.PlatformInfoId);

            IQueryable<PlatformRateUnit> query = _dbcontext.Set<PlatformRateUnit>();

            var searchString = message.SearchText ?? message.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchFor = searchString.ToLowerInvariant();
                query = query.Where(q =>
                    q.PlatformRateId == message.PlatformRateId
                    && (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)));
            }
            else
            {
                query = query.Where(q => q.PlatformRateId == message.PlatformRateId);
            }

            if (!string.IsNullOrEmpty(message.SortOrder))
            {
                query = query.OrderBy(message.SortOrder);
            }
            else query = query
                .OrderBy(o => o.SetupUnit.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;
            var results = await query
                .Include(i => i.PlatformRate)
                .Include(i => i.SetupUnit)
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new ListResult
            {
                PlatformRateId = message.PlatformRateId,
                Platform = platform,
                Product = product,
                Rate = rate,
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

    public static CreateCommand CreateNew(
        PlatformRate rate
        ) => new()
    {
        Id = Ulid.NewUlid(),
        PlatformInfoId = rate.PlatformInfoId,
        PlatformProductId = rate.PlatformProductId,
        PlatformRateId = rate.Id
    };

    public sealed record CreateQuery : IRequest<Result<CreateCommand>>
    {
        public Ulid PlatformRateId { get; set; } = Ulid.Empty;
    }

    public sealed class CreateQueryValidator : AbstractValidator<CreateQuery>
    {
        public CreateQueryValidator()
        {
            RuleFor(m => m.PlatformRateId).NotEmpty().NotEqual(Ulid.Empty);
        }
    }

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>> 
    {
        public PlatformInfo Platform { get; set; } = new();

        public PlatformProduct Product { get; set; } = new();

        public PlatformRate Rate { get; set; } = new();

        public List<SetupUnit> SetupUnits { get; set; } = [];
    }

    public sealed class CreateCommandValidator : ModelValidator<CreateCommand>
    {
        public CreateCommandValidator(FeatureContext dbContext) : base()
        {
            RuleFor(x => x.PlatformRateId).MustAsync(async (id, cancellation) =>
            {
                bool exists = await dbContext.Set<PlatformRate>().AnyAsync(x => x.Id == id, cancellation);
                return exists;
            })
            .WithMessage(m => Messages.INVALID_PLATFORMRATE_ID.Replace("{0}", m.PlatformRateId.ToString()));
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
            var rate = await _dbcontext.Set<PlatformRate>().FindAsync([message.PlatformRateId], cancellationToken: token);
            if (rate is null)
                return ProductNotFound(message.PlatformRateId);

            var product = await _dbcontext.Set<PlatformProduct>().FirstOrDefaultAsync(q => q.Id == rate.PlatformProductId, cancellationToken: token);
            if (product == null)
                return ProductNotFound(rate.PlatformProductId);

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == rate.PlatformInfoId, cancellationToken: token);
            if (platform == null)
                return PlatformNotFound(rate.PlatformInfoId);

            var record = CreateNew(rate);
            record.Platform = platform;
            record.Product = product;
            record.PlatformRate = rate;
            record.Rate = rate;
            record.SetupUnits = await GetSetupUnitsAsync(_dbcontext, token);

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
            var rate = await _dbcontext.Set<PlatformRate>().FindAsync([message.PlatformRateId], cancellationToken: token);
            if (rate is null)
                return PlatformNotFound(message.PlatformRateId);

            var unit = await _dbcontext.Set<SetupUnit>().FindAsync([message.SetupUnitId], cancellationToken: token);
            if (unit is null)
                return UnitNotFound(message.SetupUnitId);

            var record = new PlatformRateUnit
            {
                Id = message.Id,
                PlatformInfoId = message.PlatformInfoId,
                PlatformProductId = message.PlatformProductId,
                PlatformRate = rate,
                SetupUnit = unit,
                Description = message.Description,
                SearchIndex = message.SearchIndex?.Trim(),
                CreatedDT = DateTime.UtcNow,

                UnitFactor = decimal.Round(message.UnitFactor, 6, MidpointRounding.ToEven),
                DefaultValue = decimal.Round(message.DefaultValue, 6, MidpointRounding.ToEven),
            };

            await _dbcontext.Set<PlatformRateUnit>().AddAsync(record, token);
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

        public PlatformRate Rate { get; set; } = new();

        public List<SetupUnit> SetupUnits { get; set; } = [];
    }

    public class UpdateCommandValidator : ModelValidator<UpdateCommand> 
    {
        public UpdateCommandValidator(FeatureContext dbContext)
        {
            RuleFor(x => x.PlatformRateId).MustAsync(async (id, cancellation) =>
            {
                bool exists = await dbContext.Set<PlatformRate>().AnyAsync(x => x.Id == id, cancellation);
                return exists;
            })
            .WithMessage(m => Messages.INVALID_PLATFORMRATE_ID.Replace("{0}", m.PlatformRateId.ToString()));
        }
    }

    internal class UpdateCommandMapping : Profile
    {
        public UpdateCommandMapping() => 
            CreateProjection<PlatformRateUnit, UpdateCommand>();
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
            var record = await _dbcontext.Set<PlatformRateUnit>()
                .Include(i => i.PlatformRate)
                .Include(i => i.SetupUnit)
                .ProjectTo<UpdateCommand>(_configuration)
                .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

            if (record == null) 
                return RecordNotFound(message.Id);

            var rate = await _dbcontext.Set<PlatformRate>().FindAsync([record.PlatformRateId], cancellationToken: token);
            if (rate is null)
                return RateNotFound(record.PlatformRateId);
            
            var product = await _dbcontext.Set<PlatformProduct>().FindAsync([record.PlatformProductId], cancellationToken: token);
            if (product is null)
                return ProductNotFound(record.PlatformProductId);

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
            if (platform == null)
                return PlatformNotFound(product.PlatformInfoId);

            record.Platform = platform;
            record.Product = product;
            record.Rate = rate; 
            record.SetupUnits = await GetSetupUnitsAsync(_dbcontext, token);

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
            var record = await _dbcontext.Set<PlatformRateUnit>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            var unit = await _dbcontext.Set<SetupUnit>().FindAsync([message.SetupUnitId], cancellationToken: token);
            if (unit is null)
                return UnitNotFound(message.SetupUnitId);

            record.Description = message.Description.Trim();
            record.SearchIndex = message.SearchIndex?.Trim();
            record.ModifiedDT = DateTime.UtcNow;

            record.SetupUnit = unit;
            record.UnitFactor = decimal.Round(message.UnitFactor, 6, MidpointRounding.ToEven);
            record.DefaultValue = decimal.Round(message.DefaultValue, 6, MidpointRounding.ToEven);

            _dbcontext.Set<PlatformRateUnit>().Update(record);
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

        public PlatformRate Rate { get; set; } = new();

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
            CreateProjection<PlatformRateUnit, DeleteCommand>();
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
            var record = await _dbcontext.Set<PlatformRateUnit>()
                .Include(i => i.PlatformRate)
                .Include(i => i.SetupUnit)
                .ProjectTo<DeleteCommand>(_configuration)
                .FirstOrDefaultAsync(s => s.Id == message.Id, cancellationToken: token);

            if (record == null)
                return RecordNotFound(message.Id);

            var rate = await _dbcontext.Set<PlatformRate>().FindAsync([record.PlatformRateId], cancellationToken: token);
            if (rate is null)
                return RateNotFound(record.PlatformRateId);

            var product = await _dbcontext.Set<PlatformProduct>().FindAsync([record.PlatformProductId], cancellationToken: token);
            if (product is null)
                return ProductNotFound(record.PlatformProductId);

            var platform = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(q => q.Id == product.PlatformInfoId, cancellationToken: token);
            if (platform == null)
                return PlatformNotFound(product.PlatformInfoId);

            record.Platform = platform;
            record.Product = product;
            record.Rate = rate;
            record.SetupUnits = await GetSetupUnitsAsync(_dbcontext, token);

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
            var record = await _dbcontext.Set<PlatformRateUnit>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            _dbcontext.Set<PlatformRateUnit>().Remove(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }
}
