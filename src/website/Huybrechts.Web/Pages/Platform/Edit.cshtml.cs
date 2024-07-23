using Huybrechts.App.Features.Platform;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Platform
{
    public class EditModel : PageModel
    {
        private readonly IMediator _mediator;

        public EditModel(IMediator mediator) => _mediator = mediator;

        [BindProperty]
        public EditFlow.Command Data { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(EditFlow.Query query)
        {
            Data = await _mediator.Send(query);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            return this.RedirectToPage(nameof(Index));
        }
    }
}
