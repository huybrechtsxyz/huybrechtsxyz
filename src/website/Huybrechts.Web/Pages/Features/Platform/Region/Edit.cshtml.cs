using Huybrechts.App.Web;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformRegionFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Region;

[Authorize(Policy = TenantPolicies.IsManager)]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.UpdateCommand Data { get; set; }

    public IList<PlatformInfo> Platforms { get; set; } = [];

    public string PlatformInfoName { get; set; } = string.Empty;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public EditModel(IMediator mediator)
    {
        _mediator = mediator;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync(Ulid Id)
    {
        var result = await _mediator.Send(request: new Flow.UpdateQuery
        {
            Id = Id
        });
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
            return this.RedirectToPage(nameof(Index));
        }

        Platforms = result.Value.Platforms;
        PlatformInfoName = result.Value.PlatformInfoName;
        Data = result.Value;
        return Page();
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
