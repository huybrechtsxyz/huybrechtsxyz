using Huybrechts.App.Web;
using Huybrechts.Core.Extensions;
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

    public async Task<IActionResult> OnGetAsync(Flow.DeleteQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            StatusMessage = result.ToStatusMessage();
            Data = result.Value;
            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("Error", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await _mediator.Send(Data);
            StatusMessage = result.ToStatusMessage();
            return RedirectToPage(nameof(Index));
        }
        catch (Exception)
        {
            return RedirectToPage("Error", StatusCodes.Status500InternalServerError);
        }
    }
}
