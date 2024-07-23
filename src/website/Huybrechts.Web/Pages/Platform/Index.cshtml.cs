using Huybrechts.App.Features.Platform;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Platform
{
    [Authorize(Policy = TenantPolicies.IsGuest)]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;

        public IndexFlow.Result Data { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public IndexModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageIndex)
        {
            Data = await _mediator.Send(request: new IndexFlow.Query { CurrentFilter = currentFilter, Page = pageIndex, SearchString = searchString, SortOrder = sortOrder });
        }
    }
}
