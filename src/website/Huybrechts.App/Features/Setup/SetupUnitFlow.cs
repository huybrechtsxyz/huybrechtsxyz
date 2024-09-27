using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.EntityFlow;
using Huybrechts.Core.Setup;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Huybrechts.App.Features.Setup.SetupUnitFlow;


/// <summary>
/// Represents the data model for a measurement unit.
/// </summary>
/// <remarks>
/// This class is used to transfer data related to a measurement unit, including properties like code, name, 
/// description, unit type, precision, and conversion factor.
/// </remarks>
public record Model : EntityModel
{
    /// <summary>
    /// Gets or sets the code representing the unit (e.g., HOUR, KG).
    /// </summary>
    /// <remarks>
    /// The code is unique within its type and is used to standardize the representation of units.
    /// </remarks>
    [Display(Name = nameof(Code), ResourceType = typeof(Localization))]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the unit.
    /// </summary>
    /// <remarks>
    /// Name provides a human-readable description of the unit, which is unique within its type.
    /// </remarks>
    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the unit.
    /// </summary>
    /// <remarks>
    /// Provides additional information about the unit, such as its use case or any specific notes.
    /// </remarks>
    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.).
    /// </summary>
    /// <remarks>
    /// Type defines the category to which the unit belongs, allowing for better organization and lookup.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Display(Name = nameof(UnitType), ResourceType = typeof(Localization))]
    public SetupUnitType UnitType { get; set; }

    /// <summary>
    /// Gets or sets the number of decimal places of precision for this unit.
    /// </summary>
    /// <remarks>
    /// Precision determines the number of decimal places allowed for the unit's values.
    /// </remarks>
    [Display(Name = nameof(Precision), ResourceType = typeof(Localization))]
    public int Precision { get; set; } = 2;

    /// <summary>
    /// Gets or sets the precision type for rounding values of this unit.
    /// </summary>
    /// <remarks>
    /// Uses the System.Decimal Rounding enum to define rounding behavior.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Display(Name = nameof(PrecisionType), ResourceType = typeof(Localization))]
    public MidpointRounding PrecisionType { get; set; } = MidpointRounding.ToEven;

    /// <summary>
    /// Gets or sets the multiplication factor relative to the base unit.
    /// </summary>
    /// <remarks>
    /// Factor is used to convert values of this unit to the base unit within its type.
    /// </remarks>
    [Precision(18, 10)]
    [Display(Name = nameof(Factor), ResourceType = typeof(Localization))]
    public decimal Factor { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets a value indicating whether this unit is the base unit for its type.
    /// </summary>
    /// <remarks>
    /// The base unit has a factor of 1.0 and is used as the standard for conversions within the type.
    /// </remarks>
    [Display(Name = nameof(IsBase), ResourceType = typeof(Localization))]
    public bool IsBase { get; set; } = false;

    /// <summary>
    /// Additional notes or remarks related to the unit.
    /// </summary>
    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    /// <summary>
    /// Gets a normalized string for searching based on the unit's name, type, and description.
    /// </summary>
    /// <remarks>
    /// This field is constructed to provide a lower-cased concatenation of Name, UnitType, and Description,
    /// which can be used for efficient searching.
    /// </remarks>
    public string SearchIndex => $"{Name}~{UnitType}~{Description}".ToLowerInvariant();
}

/// <summary>
/// Validator for the <see cref="Model"/> class.
/// </summary>
public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel: Model
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidator"/> class.
    /// </summary>
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.Code).NotEmpty().Length(1, 10);
        RuleFor(m => m.Name).NotEmpty().Length(1, 128);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

/// <summary>
/// Provides helper methods for managing SetupUnit entities.
/// </summary>
public static class SetupUnitHelper
{
    private static SetupUnit? _defaultSetupUnit;

    /// <summary>
    /// Converts a value from one measurement unit to another.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="fromUnit">The unit to convert from.</param>
    /// <param name="toUnit">The unit to convert to.</param>
    /// <returns>The converted value, rounded to the precision of the target unit.</returns>
    /// <exception cref="ArgumentException">Thrown when the unit types are incompatible.</exception>
    public static decimal ConvertUnit(decimal value, SetupUnit fromUnit, SetupUnit toUnit)
    {
        if (fromUnit.UnitType != toUnit.UnitType)
            throw new ArgumentException("Incompatible unit types");

        decimal baseValue = value * fromUnit.Factor;
        decimal convertedValue = baseValue / toUnit.Factor;

        return Math.Round(convertedValue, toUnit.Precision, toUnit.PrecisionType);
    }

    /// <summary>
    /// Creates a new command for creating a SetupUnit.
    /// </summary>
    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    /// <summary>
    /// Retrieves all SetupUnit records from the database, ordered by UnitType and Name.
    /// </summary>
    /// <param name="dbcontext">The database context.</param>
    /// <param name="token">A cancellation token.</param>
    /// <returns>A list of SetupUnits.</returns>
    public static async Task<List<SetupUnit>> GetSetupUnitsAsync(FeatureContext dbcontext, CancellationToken token)
    {
        return await dbcontext.Set<SetupUnit>()
            .OrderBy(o => o.UnitType).ThenBy(o => o.Name)
            .ToListAsync(cancellationToken: token);
    }

    /// <summary>
    /// Finds or creates the default SetupUnit.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="save">Indicates whether to save changes.</param>
    /// <param name="token">A cancellation token.</param>
    /// <returns>The default SetupUnit.</returns>
    public static async Task<SetupUnit> GetOrCreateDefaultSetupUnitAsync(FeatureContext context, bool save, CancellationToken token)
    {
        if (_defaultSetupUnit is not null)
            return _defaultSetupUnit;

        SetupUnit? record = await context.Set<SetupUnit>().FirstOrDefaultAsync(f => f.Code == "DEFAULT", cancellationToken: token);
        if (record is not null)
            return record;

        record = new()
        {
            Id = Ulid.NewUlid(),
            Code = "DEFAULT",
            Name = "Default",
            Factor = 1,
            IsBase = false,
            Precision = 0,
            PrecisionType = MidpointRounding.ToEven,
            UnitType = SetupUnitType.System,
            Remark = null,
            SearchIndex = "default",
            Description = "Default unit",
            CreatedDT = DateTime.UtcNow,
        };

        context.Set<SetupUnit>().Add(record);
        if (save)
            await context.SaveChangesAsync(token);
        _defaultSetupUnit = record;
        return record;
    }

    /// <summary>
    /// Creates a failure result indicating that a setup unit with the specified ID was not found.
    /// </summary>
    /// <param name="id">The ID of the setup unit that was not found.</param>
    /// <returns>A <see cref="Result"/> object indicating failure with a message that includes the invalid ID.</returns>
    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_SETUPUNIT_ID.Replace("{0}", id.ToString()));

    /// <summary>
    /// Creates a failure result indicating that a duplicate setup unit name was found.
    /// </summary>
    /// <param name="name">The name of the setup unit that is a duplicate.</param>
    /// <returns>A <see cref="Result"/> object indicating failure with a message that includes the duplicate name.</returns>
    internal static Result DuplicateFound(string name) => Result.Fail(Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", name.ToString()));

    /// <summary>
    /// Asynchronously checks if a setup unit name is a duplicate within the database context.
    /// </summary>
    /// <param name="context">The database context to query for setup units.</param>
    /// <param name="name">The name to check for duplicates.</param>
    /// <param name="currentId">The ID of the current setup unit (if any), to exclude it from the check.</param>
    /// <returns>A <see cref="Task{bool}"/> that represents the asynchronous operation. The task result contains a boolean indicating whether a duplicate name exists.</returns>
    /// <remarks>
    /// This method performs a case-insensitive check by converting the name to lowercase and trimming whitespace.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();

        return await context.Set<SetupUnit>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                            && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<SetupUnit, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> 
{
    public string? SearchCode { get; set; }
}

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

public sealed record ListResult : EntityFlow.ListResult<ListModel> { }

internal sealed class ListHandler :
    EntityFlow.ListHandler<SetupUnit, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        IQueryable<SetupUnit> query = _dbcontext.Set<SetupUnit>();

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (string.IsNullOrEmpty(message.SearchCode))
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchFor = searchString.ToLower();
                query = query.Where(q => q.SearchIndex != null && q.SearchIndex.Contains(searchFor));
            }
        }
        else
        {
            query = query.Where(q => q.Code == message.SearchCode.ToUpper());
            searchString = message.SearchCode;
        }

        if (!string.IsNullOrEmpty(message.SortOrder))
        {
            query = query.OrderBy(message.SortOrder);
        }
        else query = query.OrderBy(o => o.Name);

        int pageSize = EntityFlow.ListQuery.PageSize;
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


public sealed record CreateCommand : Model, IRequest<Result<Ulid>> { }

public sealed class CreateValidator : ModelValidator<CreateCommand> 
{
    public CreateValidator(FeatureContext dbContext) :
        base()
    {
        RuleFor(x => x.Name).MustAsync(async (name, cancellation) =>
        {
            bool exists = await SetupUnitHelper.IsDuplicateNameAsync(dbContext, name);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", x.Name.ToString()));
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
        if (await SetupUnitHelper.IsDuplicateNameAsync(_dbcontext, message.Name))
            return SetupUnitHelper.DuplicateFound(message.Name);

        var record = new SetupUnit
        {
            Id = message.Id,
            Code = message.Code.ToUpper().Trim(),
            Name = message.Name.Trim(),
            Description = message.Description?.Trim(),
            UnitType = message.UnitType,
            Factor = message.Factor,
            IsBase = message.IsBase,
            Precision = message.Precision,
            PrecisionType = message.PrecisionType,
            Remark = message.Remark?.Trim(),
            SearchIndex = message.SearchIndex,
            CreatedDT = DateTime.UtcNow
        };
        await _dbcontext.Set<SetupUnit>().AddAsync(record, token);
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
        RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        {
            bool exists = await SetupUnitHelper.IsDuplicateNameAsync(dbContext, rec.Name, rec.Id);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_SETUPUNIT_NAME.Replace("{0}", x.Name.ToString()));
    }
}

internal class UpdateCommandMapping : Profile 
{ 
    public UpdateCommandMapping() => CreateProjection<SetupUnit, UpdateCommand>(); 
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
        var command = await _dbcontext.Set<SetupUnit>()
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupUnitHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<SetupUnit>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return SetupUnitHelper.RecordNotFound(message.Id);

        if (await SetupUnitHelper.IsDuplicateNameAsync(_dbcontext, message.Name, record.Id))
            return SetupUnitHelper.DuplicateFound(message.Name);

        record.Code = message.Code.ToUpper().Trim();
        record.Name = message.Name.Trim();
        record.Description = message.Description?.Trim();
        record.UnitType = message.UnitType;
        record.Factor = message.Factor;
        record.IsBase = message.IsBase;
        record.Precision = message.Precision;
        record.PrecisionType = message.PrecisionType;
        record.Remark = message.Remark?.Trim();
        record.SearchIndex = message.SearchIndex;
        record.ModifiedDT = DateTime.UtcNow;

        _dbcontext.Set<SetupUnit>().Update(record);
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
        CreateProjection<SetupUnit, DeleteCommand>();
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
        var command = await _dbcontext.Set<SetupUnit>()
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return SetupUnitHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<SetupUnit>().FindAsync([message.Id], cancellationToken: token);
        if(record is null)
            return SetupUnitHelper.RecordNotFound(message.Id);

        _dbcontext.Set<SetupUnit>().Remove(record);
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

public sealed record ImportQuery : EntityFlow.ListQuery, IRequest<Result<ImportResult>>
{
}

public sealed class ImportQueryValidator : AbstractValidator<ImportQuery> { public ImportQueryValidator() { } }

public sealed record ImportResult : EntityFlow.ListResult<ImportModel> { }

public sealed record ImportCommand : IRequest<Result>
{
    public List<ImportModel> Items { get; set; } = [];
}

public sealed class ImportCommandValidator : AbstractValidator<ImportCommand> { public ImportCommandValidator() { } }

internal sealed class ImportQueryHandler :
    EntityFlow.ListHandler<SetupUnit, ImportModel>,
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

        var filePath = Path.Combine(wwwrootPath, "data", "systemunits.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The systemunits.json file was not found.", filePath);
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

        records = [.. records.OrderBy(o => o.UnitType).ThenBy(o => o.Name)];
        int pageSize = EntityFlow.ListQuery.PageSize;
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
            if (await SetupUnitHelper.IsDuplicateNameAsync(_dbcontext, item.Name))
                continue;

            var record = new SetupUnit
            {
                Id = Ulid.NewUlid(),
                Code = item.Code.ToUpper().Trim(),
                Name = item.Name.Trim(),
                Description = item.Description?.Trim(),
                UnitType = item.UnitType,
                Factor = item.Factor,
                IsBase = item.IsBase,
                Precision = item.Precision,
                PrecisionType = item.PrecisionType,
                Remark = item.Remark?.Trim(),
                SearchIndex = item.SearchIndex,
                CreatedDT = DateTime.UtcNow
            };

            await _dbcontext.Set<SetupUnit>().AddAsync(record, token);
            changes = true;
        }

        if (changes)
            await _dbcontext.SaveChangesAsync(token);

        return Result.Ok();
    }
}