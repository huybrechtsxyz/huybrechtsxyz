using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformProductFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Product;

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

    public async Task<IActionResult> OnPostAsync()
    {
        var selection = Data.Results.Where(q => q.IsSelected == true).ToList();

        var result = await _mediator.Send(request: new Flow.ImportCommand
        {
            PlatformInfoId = Data.PlatformInfoId,
            Items = selection
        });

        return this.RedirectToPage(nameof(Index), new { platformInfoId = Data.PlatformInfoId });
    }
}