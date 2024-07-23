using Huybrechts.App.Features.Platform;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Huybrechts.Web.Pages.Platform;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    [BindProperty]
    public CreateFlow.Command Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void OnGet()
    {
        Data = new CreateFlow.Command();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(Data);

        return this.RedirectToPage(nameof(Index));
    }
}
