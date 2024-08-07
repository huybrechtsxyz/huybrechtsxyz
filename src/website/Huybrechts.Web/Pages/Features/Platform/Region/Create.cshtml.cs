using Huybrechts.App.Web;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformRegionFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Region;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    public IList<PlatformInfo> Platforms { get; set; } = [];

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
        Data = new();
    }

    public async void OnGet(Ulid? PlatformInfoId)
    {
        var result = await _mediator.Send(request: new Flow.CreateQuery
        {
            PlatformInfoId = PlatformInfoId ?? Ulid.Empty
        });
        Data = result.Region;
        Platforms = result.Platforms;
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
