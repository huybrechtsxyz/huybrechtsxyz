using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Setup.SetupCountryFlow;

namespace Huybrechts.Web.Pages.Features.Setup.Country;

[Authorize(Policy = TenantPolicies.IsManager)]
public class DefaultModel : PageModel
{
    private readonly IMediator _mediator;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DefaultModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Flow.CreateDefaultsCommand message = new() { };

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index));
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
