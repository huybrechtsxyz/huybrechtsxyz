using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformInfoFlow;

namespace Huybrechts.Web.Pages.Features.Platform;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
        Data = new();
    }

    public void OnGet()
    {
        Data = Flow.CreateNew();
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
