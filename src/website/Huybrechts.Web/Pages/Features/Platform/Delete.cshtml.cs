using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformInfoFlow;

namespace Huybrechts.Web.Pages.Features.Platform;

[Authorize(Policy = TenantPolicies.IsManager)]
public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;

    public Flow.DeleteCommand Data { get; private set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DeleteModel(IMediator mediator) => _mediator = mediator;

    public async Task OnGetAsync(Flow.DeleteQuery query)
        => Data = await _mediator.Send(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(Data);

        return RedirectToPage(nameof(Index));
    }
}
