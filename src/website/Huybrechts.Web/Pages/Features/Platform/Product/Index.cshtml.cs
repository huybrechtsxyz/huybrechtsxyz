using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformProductFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Product;

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
        Ulid? platformInfoId,
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageIndex)
    {
        Data = await _mediator.Send(request: new Flow.ListQuery
        { 
            PlatformInfoId = platformInfoId,
            CurrentFilter = currentFilter,
            SearchText = searchText,
            SortOrder = sortOrder,
            Page = pageIndex
        });
    }
}
