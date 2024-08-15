using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformRateFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Rate;

[Authorize(Policy = TenantPolicies.IsMember)]
public class ImportModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.ImportResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public ImportModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync(
        Ulid? platformServiceId,
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageIndex,
        string currencyCode,
        Ulid? platformRegionId,
        Ulid? platformProductId)
    {
        Data = await _mediator.Send(request: new Flow.ImportQuery
        {
            PlatformServiceId = platformServiceId ?? Ulid.Empty,
            PlatformRegionId = platformRegionId,
            PlatformProductId = platformProductId,
            Currency = currencyCode,
            CurrentFilter = currentFilter,
            SearchText = searchText,
            SortOrder = sortOrder,
            Page = pageIndex
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var selection = Data.Results.Where(q => q.IsSelected == true).ToList();

        var result = await _mediator.Send(request: new Flow.ImportCommand
        {
            PlatformServiceId = Data.PlatformServiceId ?? Ulid.Empty,
            Items = selection
        });

        return this.RedirectToPage(nameof(Index), new { platformServiceId = Data.PlatformServiceId });
    }
}