using Huybrechts.App.Web;
using Huybrechts.Core.Extensions;
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

    public async Task<IActionResult> OnGetAsync(Flow.UpdateQuery query)
    {
        try
        {
            var result = await _mediator.Send(query) ?? new();
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
