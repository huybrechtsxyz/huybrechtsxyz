using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Wiki.WikiInfoFlow;

namespace Huybrechts.Web.Pages.Features.Wiki;

[Authorize(Policy = TenantPolicies.IsMember)]
public class SearchModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.SearchQuery> _validator;

    public Flow.SearchResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public SearchModel(IMediator mediator, IValidator<Flow.SearchQuery> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<IActionResult> OnGetAsync(
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageSearch)
    {
        try
        {
            var language = "english";
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
            if (requestCulture is not null)
                language = requestCulture.RequestCulture.UICulture.EnglishName.ToLower();
            else
                language = "english";

            var message = new Flow.SearchQuery()
            {
                Language = language,
                CurrentFilter = currentFilter,
                SearchText = searchText,
                SortOrder = sortOrder,
                Page = pageSearch
            };

            ValidationResult state = await _validator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            Data = result.Value;
            return Page();
        }
        catch(Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
