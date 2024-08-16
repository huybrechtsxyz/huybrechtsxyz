using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformInfoFlow;

namespace Huybrechts.Web.Pages.Features.Platform;

[Authorize(Policy = TenantPolicies.IsManager)]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.UpdateCommand Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public EditModel(IMediator mediator) => _mediator = mediator;

    public async Task OnGetAsync(Flow.UpdateQuery query)
    {
        var result = await _mediator.Send(query) ?? new();
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
        }
        Data = result.Value;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _mediator.Send(Data);
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
        }
        return this.RedirectToPage(nameof(Index));
    }
}
