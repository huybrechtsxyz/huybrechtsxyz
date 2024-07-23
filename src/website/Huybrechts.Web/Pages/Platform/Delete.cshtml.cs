using Huybrechts.App.Features.Platform;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Platform;

public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;

    public DeleteFlow.Model Data { get; private set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DeleteModel(IMediator mediator) => _mediator = mediator;

    public async Task OnGetAsync(DeleteFlow.Query query)
        => Data = await _mediator.Send(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _mediator.Send(Data);

        return this.RedirectToPage(nameof(Index));
    }
}
