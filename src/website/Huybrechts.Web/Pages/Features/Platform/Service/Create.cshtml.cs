using Huybrechts.App.Web;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformServiceFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Service;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    public IList<PlatformInfo> Platforms { get; set; } = [];

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformProduct> Products { get; set; } = [];

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync(Ulid PlatformInfoId)
    {
        var result = await _mediator.Send(request: new Flow.CreateQuery
        {
            PlatformInfoId = PlatformInfoId
        });
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
            return this.RedirectToPage(nameof(Index));
        }

        Platforms = result.Value.Platforms;
        Regions = result.Value.Regions;
        Products = result.Value.Products;
        Data = result.Value.Service;
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
