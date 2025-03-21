﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace Huybrechts.App.Features.Platform.PlatformInfoFlow;

public static class PlatformInfoHelper
{
    public static CreateCommand CreateNew() => new() { Id = Ulid.NewUlid() };

    internal static Result RecordNotFound(Ulid id) => Result.Fail(Messages.INVALID_PLATFORM_ID.Replace("{0}", id.ToString()));

    internal static Result DuplicateFound(string name) => Result.Fail(Messages.DUPLICATE_PLATFORM_NAME.Replace("{0}", name.ToString()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public static async Task<bool> IsDuplicateNameAsync(DbContext context, string name, Ulid? currentId = null)
    {
        name = name.ToLower().Trim();

        return await context.Set<PlatformInfo>()
            .AnyAsync(pr => pr.Name.ToLower() == name
                            && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

public record Model
{
    public Ulid Id { get; init; }

    [Display(Name = nameof(Name), ResourceType = typeof(Localization))]
    public string Name { get; set; } = string.Empty;

    [Display(Name = nameof(Description), ResourceType = typeof(Localization))]
    public string? Description { get; set; }

    [Display(Name = nameof(Provider), ResourceType = typeof(Localization))]
    public PlatformProvider Provider { get; set; }

    [Display(Name = nameof(Remark), ResourceType = typeof(Localization))]
    public string? Remark { get; set; }

    public string SearchIndex => $"{Name}~{Description}".ToLowerInvariant();
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.Name).NotNull().Length(1, 128);
        RuleFor(m => m.Description).Length(0, 256);
    }
}

//
// LIST
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<PlatformInfo, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> { }

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

public sealed record ListResult : EntityFlow.ListResult<ListModel> { }

internal sealed class ListHandler :
    EntityFlow.ListHandler<PlatformInfo, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        IQueryable<PlatformInfo> query = _dbcontext.Set<PlatformInfo>();

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

public sealed record CreateCommand : Model, IRequest<Result<Ulid>>
{
    public bool SkipIfExists { get; set; } = false;
}

public sealed class CreateValidator : ModelValidator<CreateCommand> 
{
    public CreateValidator(FeatureContext dbContext) :
        base()
    {
        RuleFor(x => x.Name).MustAsync(async (name, cancellation) =>
        {
            bool exists = await PlatformInfoHelper.IsDuplicateNameAsync(dbContext, name);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_PLATFORM_NAME.Replace("{0}", x.Name.ToString()));
    }
}

internal sealed class CreateHandler : IRequestHandler<CreateCommand, Result<Ulid>>
{
    private readonly FeatureContext _dbcontext;

    public CreateHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "EntityFrameworkCore")]
    public async Task<Result<Ulid>> Handle(CreateCommand message, CancellationToken token)
    {
        if (await PlatformInfoHelper.IsDuplicateNameAsync(_dbcontext, message.Name))
            if (message.SkipIfExists)
            {
                var name = message.Name.ToLower().Trim();
                var item = await _dbcontext.Set<PlatformInfo>().FirstAsync(pr => pr.Name.ToLower() == name, token);
                return Result.Ok(item.Id);
            }
            else
                return PlatformInfoHelper.DuplicateFound(message.Name);

        var record = new PlatformInfo
        {
            Id = message.Id,
            Name = message.Name.Trim(),
            Description = message.Description?.Trim(),
            Provider = message.Provider,
            Remark = message.Remark?.Trim(),
            SearchIndex = message.SearchIndex,
            CreatedDT = DateTime.UtcNow
        };
        await _dbcontext.Set<PlatformInfo>().AddAsync(record, token);
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
            bool exists = await PlatformInfoHelper.IsDuplicateNameAsync(dbContext, rec.Name, rec.Id);
            return !exists;
        }).WithMessage(x => Messages.DUPLICATE_PLATFORM_NAME.Replace("{0}", x.Name.ToString()));
    }
}

internal class UpdateCommandMapping : Profile 
{ 
    public UpdateCommandMapping() => CreateProjection<PlatformInfo, UpdateCommand>(); 
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
        var command = await _dbcontext.Set<PlatformInfo>()
            .ProjectTo<UpdateCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return PlatformInfoHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<PlatformInfo>().FindAsync([message.Id], cancellationToken: token);
        if (record is null)
            return PlatformInfoHelper.RecordNotFound(message.Id);

        if (await PlatformInfoHelper.IsDuplicateNameAsync(_dbcontext, message.Name, record.Id))
            return PlatformInfoHelper.DuplicateFound(message.Name);

        record.Name = message.Name.Trim();
        record.Description = message.Description?.Trim();
        record.Provider = message.Provider;
        record.Remark = message.Remark?.Trim();
        record.SearchIndex = message.SearchIndex;
        record.ModifiedDT = DateTime.UtcNow;

        _dbcontext.Set<PlatformInfo>().Update(record);
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
        CreateProjection<PlatformInfo, DeleteCommand>();
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
        var command = await _dbcontext.Set<PlatformInfo>()
            .ProjectTo<DeleteCommand>(_configuration)
            .FirstOrDefaultAsync(q => q.Id == message.Id, cancellationToken: token);

        if (command is null)
            return PlatformInfoHelper.RecordNotFound(message.Id ?? Ulid.Empty);

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
        var record = await _dbcontext.Set<PlatformInfo>().FindAsync([message.Id], cancellationToken: token);
        if(record is null)
            return PlatformInfoHelper.RecordNotFound(message.Id);

        _dbcontext.Set<PlatformInfo>().Remove(record);
        await _dbcontext.SaveChangesAsync(token);
        return Result.Ok();
    }
}
