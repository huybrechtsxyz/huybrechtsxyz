using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformRegionFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Region;

[Authorize(Policy = TenantPolicies.IsMember)]
public class ImportModel : PageModel
{
    private readonly IMediator _mediator;

    public Flow.ImportResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public ImportModel(IMediator mediator)
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
        Data = await _mediator.Send(request: new Flow.ImportQuery
        {
            PlatformInfoId = platformInfoId ?? Ulid.Empty,
            CurrentFilter = currentFilter,
            SearchText = searchText,
            SortOrder = sortOrder,
            Page = pageIndex
        });
    }
}