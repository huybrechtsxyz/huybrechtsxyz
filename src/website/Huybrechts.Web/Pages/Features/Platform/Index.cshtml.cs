using Huybrechts.App.Features.Platform.Info;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Features.Platform;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexFlow.Result Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnGetAsync(string sortOrder,
            string currentFilter,
            string searchText,
            int? pageIndex)
    {
        Data = await _mediator.Send(request: new IndexFlow.Query { 
            CurrentFilter = currentFilter,
            SearchText = searchText,
            Page = pageIndex
        });
    }
}
