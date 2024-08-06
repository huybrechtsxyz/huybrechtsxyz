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

    [BindProperty]
    public Flow.DeleteCommand Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DeleteModel(IMediator mediator) => _mediator = mediator;

    public async Task OnGetAsync(Flow.DeleteQuery query)
    {
        Data = await _mediator.Send(query);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _mediator.Send(Data);
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
        }
        return RedirectToPage(nameof(Index));
    }
}
