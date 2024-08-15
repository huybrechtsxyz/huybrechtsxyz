using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformRateFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Rate;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.ListResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync(
        Ulid? platformServiceId,
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageIndex)
    {
        Data = await _mediator.Send(request: new Flow.ListQuery
        { 
            PlatformServiceId = platformServiceId,
            CurrentFilter = currentFilter,
            SearchText = searchText,
            SortOrder = sortOrder,
            Page = pageIndex
        });
    }
}
