using Huybrechts.App.Web;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformRateFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Rate;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    public IList<PlatformRegion> Regions { get; set; } = [];

    public IList<PlatformProduct> Products { get; set; } = [];

    public List<string> Currencies { get; set; } = [];

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync(Ulid PlatformServiceId)
    {
        var result = await _mediator.Send(request: new Flow.CreateQuery
        {
            PlatformServiceId = PlatformServiceId
        });
        if (result.IsFailed)
        {
            StatusMessage = result.Errors[0].Message;
            return this.RedirectToPage(nameof(Index));
        }

        Regions = result.Value.Regions;
        Products = result.Value.Products;
        Currencies = [.. result.Value.Currencies];
        Data = result.Value.Item;
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
