using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace Huybrechts.App.Features.Setup;

public static class SetupCountryFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Code), ResourceType = typeof(Localization))]
        public string Code { get; set; } = string.Empty;

        [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
        public string? Description { get; set; }

        [Display(Name = nameof(TranslatedName), ResourceType = typeof(Localization))]
        public string TranslatedName { get; set; } = string.Empty;

        public string SearchIndex => $"{Code}~{Name}~{TranslatedName}".ToUpperInvariant();
    }

    public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
    {
        public ModelValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
            RuleFor(m => m.Code).NotEmpty().Length(1, 10);
            RuleFor(m => m.Name).NotEmpty().Length(1, 128);
            RuleFor(m => m.Description).Length(0, 256);
            RuleFor(m => m.TranslatedName).NotEmpty().Length(1, 128);
        }
    }

    private static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPCOUNTRY_ID.Replace("{0}", id.ToString()));

    private static Result DuplicateCodeFound(string code) => Result.Fail(Messages.DUPLICATE_SETUPCOUNTRY_CODE.Replace("{0}", code.ToString()));

    private static Result DuplicateNameFound(string name) => Result.Fail(Messages.DUPLICATE_SETUPCOUNTRY_NAME.Replace("{0}", name.ToString()));

    public static async Task<bool> IsDuplicateCodeAsync(DbContext context, string code, Ulid? currentId = null)
    {
        code = code.ToUpper().Trim();

        return await context.Set<SetupCountry>()
            .AnyAsync(pr => pr.Code.ToUpper() == code
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();

        return await context.Set<SetupCountry>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                         && (!currentId.HasValue || pr.Id != currentId.Value));
    }

    //
    // LIST
    //

    public sealed record ListModel : Model { }

    internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<SetupCountry, ListModel>(); }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<Result<ListResult>> { }

    public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel> { }

    internal sealed class ListHandler :
        EntityListFlow.Handler<SetupCountry, ListModel>,
        IRequestHandler<ListQuery, Result<ListResult>>
    {
        public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration)
        {
        }

        public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
        {
            IQueryable<SetupCountry> query = _dbcontext.Set<SetupCountry>();

            var searchString = message.SearchText ?? message.CurrentFilter;
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchFor = searchString.ToLower();
                query = query.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor));
            }

            if (!string.IsNullOrEmpty(message.SortOrder))
            {
                query = query.OrderBy(message.SortOrder);
            }
            else query = query.OrderBy(o => o.Name);

            int pageSize = EntityListFlow.PageSize;
            int pageNumber = message.Page ?? 1;
            var results = await query
                .ProjectTo<ListModel>(_configuration)
                .PaginatedListAsync(pageNumber, pageSize);

            var model = new ListResult
            {
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

    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

    public sealed class CreateValidator : ModelValidator<CreateCommand> 
    {
        public CreateValidator(FeatureContext dbContext) :
            base()
        {
            RuleFor(x => x.Code).MustAsync(async (code, cancellation) =>
            {
                bool exists = await IsDuplicateCodeAsync(dbContext, code);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPCOUNTRY_CODE.Replace("{0}", x.Code.ToString()));

            RuleFor(x => x.Name).MustAsync(async (name, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, name);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPCOUNTRY_NAME.Replace("{0}", x.Name.ToString()));
        }
    }

    internal sealed class CreateHandler : IRequestHandler<CreateCommand, Result<Ulid>>
    {
        private readonly FeatureContext _dbcontext;

        public CreateHandler(FeatureContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Result<Ulid>> Handle(CreateCommand message, CancellationToken token)
        {
            if (await IsDuplicateCodeAsync(_dbcontext, message.Code))
                return DuplicateCodeFound(message.Code);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name))
                return DuplicateNameFound(message.Name);

            var record = new SetupCountry
            {
                Id = message.Id,
                Code = message.Code.ToUpper().Trim(),
                Name = message.Name.Trim(),
                Description = message.Description?.Trim(),
                SearchIndex = message.SearchIndex,
                CreatedDT = DateTime.UtcNow,

                TranslatedName = message.TranslatedName
            };
            await _dbcontext.Set<SetupCountry>().AddAsync(record, token);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok(record.Id);
        }
    }

    //
    // UPDATE
    //

    public sealed record UpdateQuery : IRequest<Result<UpdateCommand>> { public Ulid? Id { get; init; } }

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
            RuleFor(x => x.Code).MustAsync(async (code, cancellation) =>
            {
                bool exists = await IsDuplicateCodeAsync(dbContext, code);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPCOUNTRY_CODE.Replace("{0}", x.Code.ToString()));

            RuleFor(x => x).MustAsync(async (rec, cancellation) =>
            {
                bool exists = await IsDuplicateNameAsync(dbContext, rec.Name, rec.Id);
                return !exists;
            }).WithMessage(x => Messages.DUPLICATE_SETUPLANGUAGE_NAME.Replace("{0}", x.Name.ToString()));
        }
    }

    internal class UpdateCommandMapping : Profile 
    { 
        public UpdateCommandMapping() => CreateProjection<SetupCountry, UpdateCommand>(); 
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
            var command = await _dbcontext.Set<SetupCountry>()
                .ProjectTo<UpdateCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return RecordNotFound(message.Id ?? Ulid.Empty);

            return Result.Ok(command);
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
            var record = await _dbcontext.Set<SetupCountry>().FindAsync([message.Id], cancellationToken: token);
            if (record is null)
                return RecordNotFound(message.Id);

            if (await IsDuplicateCodeAsync(_dbcontext, message.Code, record.Id))
                return DuplicateCodeFound(message.Code);

            if (await IsDuplicateNameAsync(_dbcontext, message.Name, record.Id))
                return DuplicateNameFound(message.Name);

            record.Code = message.Code.ToUpper().Trim();
            record.Name = message.Name.Trim();
            record.Description = message.Description?.Trim();
            record.SearchIndex = message.SearchIndex;
            record.ModifiedDT = DateTime.UtcNow;

            record.TranslatedName = message.TranslatedName;

            _dbcontext.Set<SetupCountry>().Update(record);
            await _dbcontext.SaveChangesAsync(token);
            return Result.Ok();
        }
    }

    //
    // DELETE
    //

    public sealed record DeleteQuery : IRequest<Result<DeleteCommand>> { public Ulid? Id { get; init; } }

    public class DeleteQueryValidator : AbstractValidator<DeleteQuery>
    {
        public DeleteQueryValidator()
        {
            RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
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
        public DeleteCommandMapping()
        {
            CreateProjection<SetupCountry, DeleteCommand>();
        }
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
            var command = await _dbcontext.Set<SetupCountry>()
                .ProjectTo<DeleteCommand>(_configuration)
                .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

            if (command is null)
                return RecordNotFound(message.Id ?? Ulid.Empty);

            return Result.Ok(command);
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
            var record = await _dbcontext.Set<SetupCountry>().FindAsync([message.Id], cancellationToken: token);
            if(record is null)
                return RecordNotFound(message.Id);

            _dbcontext.Set<SetupCountry>().Remove(record);
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
    }

    public sealed class ImportQueryValidator : AbstractValidator<ImportQuery> { public ImportQueryValidator() { } }

    public sealed class ImportResult : EntityListFlow.Result<ImportModel>
    {
    }

    public sealed record ImportCommand : IRequest<Result>
    {
        public List<ImportModel> Items { get; set; } = [];
    }

    public sealed class ImportCommandValidator : AbstractValidator<ImportCommand> { public ImportCommandValidator() { } }

    internal sealed class ImportQueryHandler :
       EntityListFlow.Handler<SetupLanguage, ImportModel>,
       IRequestHandler<ImportQuery, Result<ImportResult>>
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImportQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration, IWebHostEnvironment webHostEnvironment)
            : base(dbcontext, configuration)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result<ImportResult>> Handle(ImportQuery message, CancellationToken token)
        {
            string wwwrootPath = _webHostEnvironment.WebRootPath;

            var filePath = Path.Combine(wwwrootPath, "data", "countries.json");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The countries.json file was not found.", filePath);
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<ImportModel>? records = await JsonSerializer.DeserializeAsync<List<ImportModel>>(stream, cancellationToken: token);
            records ??= [];

            var searchString = message.SearchText ?? message.CurrentFilter;
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

            bool changes = false;
            foreach (var item in message.Items ?? [])
            {
                if (await IsDuplicateNameAsync(_dbcontext, item.Name))
                    continue;

                var record = new SetupCountry
                {
                    Id = Ulid.NewUlid(),
                    Code = item.Code.ToUpper().Trim(),
                    Name = item.Name.Trim(),
                    TranslatedName = item.TranslatedName.Trim(),
                    Description = item.Description?.Trim(),
                    SearchIndex = item.SearchIndex,
                    CreatedDT = DateTime.UtcNow
                };

                await _dbcontext.Set<SetupCountry>().AddAsync(record, token);
                changes = true;
            }

            if (changes)
                await _dbcontext.SaveChangesAsync(token);

            return Result.Ok();
        }
    }
}
