using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentValidation;
using Huybrechts.App.Data;
using Huybrechts.App.Features.EntityFlow;
using Huybrechts.Core.Wiki;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Huybrechts.App.Features.Wiki.WikiInfoFlow;

public record Model : EntityModel
{
    public string Namespace { get; set; } = string.Empty;

    public string Page { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Tags { get; set; }

    public string Content { get; set; } = string.Empty;

    public string GetWikiUrl() => WikiInfoHelper.GetWikiUrl(
        WikiInfoHelper.GetUrlPrefix(),
        WikiInfoHelper.GetDefaultNamespace(),
        this.Namespace,
        this.Page);

    public string GetWikiUrl(string tenantid) => WikiInfoHelper.GetWikiUrl(
        WikiInfoHelper.GetUrlPrefix(tenantid),
        WikiInfoHelper.GetDefaultNamespace(),
        this.Namespace,
        this.Page);
}

public class ModelValidator<TModel> : AbstractValidator<TModel> where TModel : Model
{
    public ModelValidator()
    {
        RuleFor(m => m.Id).NotEmpty().NotEqual(Ulid.Empty);
        RuleFor(m => m.Namespace).NotEmpty().Length(1, 128);
        RuleFor(m => m.Page).NotEmpty().Length(1, 512);
        RuleFor(m => m.Title).Length(1, 512);
        RuleFor(m => m.Tags).Length(0, 256);
        RuleFor(m => m.Content).NotNull();
    }
}

public static class WikiInfoHelper
{
    public static string GetWikiUrl(string prefix, string defaultspace, string wikispace, string wikipage)
    {
        // Resolve this URL properly based on your wiki structure
        StringBuilder url = new();
        if (!string.IsNullOrEmpty(prefix))
        {
            url.Append(prefix);
            if (!prefix.EndsWith('/'))
                url.Append('/');
        }
        if (!string.IsNullOrEmpty(wikispace))
        {
            url.Append(wikispace);
            url.Append('/');
        }
        else if (!string.IsNullOrEmpty(defaultspace))
        {
            url.Append(defaultspace);
            url.Append('/');
        }
        url.Append(wikipage);
        return url.ToString();
    }

    public static string GetUrlPrefix() => $"/Features/Wiki";

    public static string GetUrlPrefix(string tenant) => $"/{tenant}/Features/Wiki/";

    public static string GetDefaultNamespace() => "wiki";

    public static string GetSearchIndex(string title, string? tags) => $"{title}~{tags}".ToLowerInvariant();

    public static void CopyFields(Model model, WikiPage entity)
    {
        entity.Namespace = model.Namespace.Trim().ToLower();
        entity.Page = model.Page.Trim().ToLower();
        entity.Title = model.Title.Trim();
        entity.Tags = model.Tags?.Trim();
        entity.Content = model.Content;
        entity.SearchIndex = GetSearchIndex(entity.Title, entity.Tags);
    }

    public static string NormalizeNamespace(string nspace)
    {
        nspace = nspace
            .Replace(" ", "-") // Replace spaces with hyphens
            .Trim() // Remove leading and trailing spaces
            .ToLower(); // Convert to lower case for consistency
        
        return Uri.EscapeDataString(nspace);
    }

    public static string NormalizePage(string page)
    {
        page = page
            .Replace(" ", "-") // Replace spaces with hyphens
            .Trim() // Remove leading and trailing spaces
            .ToLower(); // Convert to lower case for consistency

        return Uri.EscapeDataString(page);
    }

    internal static Result EntityNotFound(Ulid id) => Result.Fail(Messages.INVALID_WIKI_ID.Replace("{0}", id.ToString()));

    public static async Task<bool> IsDuplicateAsync(DbContext context, string nspace, string page, Ulid? currentId = null)
    {
        nspace = NormalizeNamespace(nspace);
        page = NormalizePage(page);

        return await context.Set<WikiPage>()
            .AnyAsync(pr => pr.Namespace == nspace
                 && pr.Page == page
                 && (!currentId.HasValue || pr.Id != currentId.Value));
    }
}

//
// TABLE OF CONTENTS
//

public sealed record ListModel : Model { }

internal sealed class ListMapping : Profile { public ListMapping() => CreateProjection<WikiPage, ListModel>(); }

public sealed record ListQuery : EntityFlow.ListQuery, IRequest<Result<ListResult>> { }

public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

public sealed record ListResult : EntityFlow.ListResult<ListModel> { }

internal sealed class ListHandler :
    EntityFlow.ListHandler<WikiPage, ListModel>,
    IRequestHandler<ListQuery, Result<ListResult>>
{
    public ListHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<ListResult>> Handle(ListQuery message, CancellationToken token)
    {
        IQueryable<WikiPage> query = _dbcontext.Set<WikiPage>();

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
        else query = query.OrderBy(o => o.Namespace).ThenBy(o => o.Title);

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
// SEARCH LIST
//

public sealed record SearchModel : Model { public float Rank { get; set; } }

public sealed record SearchQuery : EntityFlow.ListQuery, IRequest<Result<SearchResult>> 
{
    public string Language { get; set; } = string.Empty;  // 'english' or 'dutch'
}

public sealed class SearchValidator : AbstractValidator<SearchQuery> 
{
    public SearchValidator() 
    {
        RuleFor(x => x.Language).Must(language => language == "english" || language == "dutch");
    } 
}

internal sealed class SearchMapping : Profile { public SearchMapping() => CreateProjection<WikiPage, SearchModel>(); }

public sealed record SearchResult : EntityFlow.ListResult<SearchModel> { }

internal sealed class SearchHandler :
    EntityFlow.ListHandler<WikiPage, SearchModel>,
    IRequestHandler<SearchQuery, Result<SearchResult>>
{
    public SearchHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
        : base(dbcontext, configuration)
    {
    }

    public async Task<Result<SearchResult>> Handle(SearchQuery message, CancellationToken token)
    {
        if (message is null)
        {
            return new SearchResult
            {
                CurrentFilter = string.Empty,
                SearchText = string.Empty,
                SortOrder = string.Empty,
                Results = []
            };
        }

        if (string.IsNullOrEmpty(message.CurrentFilter) && string.IsNullOrEmpty(message.SearchText))
        {
            return new SearchResult
            {
                CurrentFilter = string.Empty,
                SearchText = string.Empty,
                SortOrder = message.SortOrder,
                Results = []
            };
        }

        if (_dbcontext.Database.IsNpgsql())
            return await HandleNpgsql(message, token);

        if (_dbcontext.Database.IsSqlite())
            return await HandleSqlite(message, token);

        if (_dbcontext.Database.IsSqlServer() && !_dbcontext.IsLocalSqlServer())
            return await HandleSqlServer(message, token);

        if (_dbcontext.Database.IsSqlServer() && _dbcontext.IsLocalSqlServer())
            return await HandleSqlite(message, token);
        
        throw new ApplicationException("Invalid type of database");
    }

    public async Task<Result<SearchResult>> HandleNpgsql(SearchQuery message, CancellationToken token)
    {
        var searchString = message.SearchText ?? message.CurrentFilter;
        var language = message.Language == "english" ? "english" : "dutch";

        string sql = $@"
            SELECT wp.*, 
                   ts_rank(to_tsvector('{language}', wp.""Content""), to_tsquery('{language}', @searchText)) AS RANK
            FROM ""WikiPage"" wp
            WHERE to_tsvector('{language}', wp.""Content"") @@ to_tsquery('{language}', @searchText)
            ORDER BY RANK DESC
        ";

        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;

        var results = await _dbcontext.Set<WikiPage>()
        .FromSqlRaw(sql, new NpgsqlParameter("@searchText", searchString))
        .Select(wp => new SearchModel
        {
            Id = wp.Id,
            Namespace = wp.Namespace,
            Page = wp.Page,
            Title = wp.Title,
            Tags = wp.Tags,
            Content = wp.Content,
            Rank = EF.Property<float>(wp, "RANK"), // Map RANK manually
            CreatedBy = wp.CreatedBy,
            CreatedDT = wp.CreatedDT,
            ModifiedBy = wp.ModifiedBy,
            ModifiedDT = wp.ModifiedDT
        })
        .PaginatedListAsync(pageNumber, pageSize);

        var model = new SearchResult
        {
            CurrentFilter = searchString,
            SearchText = searchString,
            SortOrder = message.SortOrder,
            Results = results ?? []
        };

        return model;
    }

    public async Task<Result<SearchResult>> HandleSqlite(SearchQuery message, CancellationToken token)
    {
        IQueryable<WikiPage> query = _dbcontext.Set<WikiPage>();

        var searchString = message.SearchText ?? message.CurrentFilter;
        if (!string.IsNullOrEmpty(searchString))
        {
            var searchFor = searchString.ToLower();
            query = query.Where(q => (q.SearchIndex != null && q.SearchIndex.Contains(searchFor)) || q.Content.Contains(searchString));
        }

        if (!string.IsNullOrEmpty(message.SortOrder))
        {
            query = query.OrderBy(message.SortOrder);
        }
        else query = query.OrderBy(o => o.Namespace).ThenBy(o => o.Title);

        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await query
            .ProjectTo<SearchModel>(_configuration)
            .PaginatedListAsync(pageNumber, pageSize);

        var model = new SearchResult
        {
            CurrentFilter = searchString,
            SearchText = searchString,
            SortOrder = message.SortOrder,
            Results = results ?? []
        };

        return model;
    }

    public async Task<Result<SearchResult>> HandleSqlServer(SearchQuery message, CancellationToken token)
    {
        var searchString = message.SearchText ?? message.CurrentFilter;

        string sql = @"SELECT wp.*, ft.RANK
                       FROM WikiPage wp
                       INNER JOIN FREETEXTTABLE(WikiPage, (Namespace, Page, Title, Tags, Content), @searchTerm) AS ft
                       ON wp.Id = ft.[KEY]
                       ORDER BY ft.RANK DESC;";

        int pageSize = EntityFlow.ListQuery.PageSize;
        int pageNumber = message.Page ?? 1;
        var results = await _dbcontext.Set<WikiPage>()
            .FromSqlRaw(sql, new SqlParameter("@searchTerm", searchString))
            .Select(wp => new SearchModel
            {
                Id = wp.Id,
                Namespace = wp.Namespace,
                Page = wp.Page,
                Title = wp.Title,
                Tags = wp.Tags,
                Content = wp.Content,
                Rank = EF.Property<int>(wp, "RANK"), // Map RANK manually
                CreatedBy = wp.CreatedBy,
                CreatedDT = wp.CreatedDT,
                ModifiedBy = wp.ModifiedBy,
                ModifiedDT = wp.ModifiedDT
            })
            .PaginatedListAsync(pageNumber, pageSize);

        var model = new SearchResult
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
// WIKI EDIT
//

public sealed record EditQuery : IRequest<Result<EditCommand>> 
{ 
    public string Namespace { get; init; } = string.Empty;

    public string Page { get; init; } = string.Empty;
}

public sealed class EditQueryValidator : AbstractValidator<EditQuery>
{
    public EditQueryValidator()
    {
        RuleFor(m => m.Namespace).Length(0, 128);
        RuleFor(m => m.Page).NotEmpty().Length(1, 512);
    }
}

public record EditCommand : Model, IRequest<Result> 
{
    //public bool IsDeletable { get; set; } = false;
}

public class EditCommandValidator : ModelValidator<EditCommand>
{
    public EditCommandValidator()
    {
        //RuleFor(x => x).MustAsync(async (rec, cancellation) =>
        //{
        //    bool exists = await WikiInfoHelper.IsDuplicateAsync(dbContext, rec.Namespace, rec.Path, rec.Id);
        //    return !exists;
        //})
        //.WithMessage(x => Messages.DUPLICATE_WIKI_PATH.Replace("{0}", $"{x.Namespace}::{x.Path}"))
        //.WithName(x => x.Path);
    }
}

internal class EditCommandMapping : Profile
{
    public EditCommandMapping() => CreateProjection<WikiPage, EditCommand>();
}

internal class EditQueryHandler : IRequestHandler<EditQuery, Result<EditCommand>>
{
    private readonly FeatureContext _dbcontext;
    private readonly IConfigurationProvider _configuration;

    public EditQueryHandler(FeatureContext dbcontext, IConfigurationProvider configuration)
    {
        _dbcontext = dbcontext;
        _configuration = configuration;
    }

    public async Task<Result<EditCommand>> Handle(EditQuery message, CancellationToken token)
    {
        string nspace = WikiInfoHelper.NormalizeNamespace(message.Namespace);
        string page = WikiInfoHelper.NormalizePage(message.Page);

        var command = await _dbcontext.Set<WikiPage>()
            .ProjectTo<EditCommand>(_configuration)
            .FirstOrDefaultAsync(x => x.Namespace == nspace && x.Page == page, token) ?? new()
            {
                Id = Ulid.NewUlid(),
                Namespace = nspace,
                Page = page,
                Title = "New wiki page",
                Tags = "",
                Content = Messages.NEW_WIKI_CONTENT
                    .Replace("{namespace}", nspace)
                    .Replace("{page}", page)
    };
        return Result.Ok(command);
    }
}

internal class EditCommandHandler : IRequestHandler<EditCommand, Result>
{
    private readonly FeatureContext _dbcontext;

    public EditCommandHandler(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<Result> Handle(EditCommand message, CancellationToken token)
    {
        string nspace = WikiInfoHelper.NormalizeNamespace(message.Namespace);
        string page = WikiInfoHelper.NormalizePage(message.Page);

        WikiPage? entity = await _dbcontext.Set<WikiPage>().FirstOrDefaultAsync(x => x.Namespace == nspace && x.Page == page, token);

        await _dbcontext.BeginTransactionAsync(token);

        if (entity is not null)
        {
            entity.ModifiedDT = DateTime.UtcNow;
            WikiInfoHelper.CopyFields(message, entity);
            _dbcontext.Set<WikiPage>().Update(entity);
        }
        else
        {
            entity = new()
            {
                Id = message.Id,
                CreatedDT = DateTime.UtcNow,
            };
            WikiInfoHelper.CopyFields(message, entity);
            await _dbcontext.Set<WikiPage>().AddAsync(entity, token);
        }

        // Manually update the full-text search vectors
        if (_dbcontext.Database.IsNpgsql())
        {
            var sql = "UPDATE \"WikiPage\" SET \"TsvEnglish\" = to_tsvector('english', \"Content\"), \"TsvDutch\" = to_tsvector('dutch', \"Content\") WHERE \"Id\" = @id;";            
            _dbcontext.Database.ExecuteSqlRaw(sql, new NpgsqlParameter("@id", entity.Id.ToString()));
        }

        await _dbcontext.CommitTransactionAsync(token);

        return Result.Ok();
    }
}

//
// WIKI DELETE
//

public record DeleteCommand : IRequest<Result>
{
    public string Namespace { get; init; } = string.Empty;

    public string Page { get; init; } = string.Empty;
}

public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
{
    public DeleteCommandValidator()
    {
        RuleFor(m => m.Namespace).Length(0, 128);
        RuleFor(m => m.Page).NotEmpty().Length(1, 512);
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
        string nspace = WikiInfoHelper.NormalizeNamespace(message.Namespace);
        string page = WikiInfoHelper.NormalizePage(message.Page);

        WikiPage? entity = await _dbcontext.Set<WikiPage>().FirstOrDefaultAsync(x => x.Namespace == nspace && x.Page == page, token);
        if (entity is not null)
        {
            _dbcontext.Set<WikiPage>().Remove(entity);
            await _dbcontext.SaveChangesAsync(token);
        }

        return Result.Ok();
    }
}