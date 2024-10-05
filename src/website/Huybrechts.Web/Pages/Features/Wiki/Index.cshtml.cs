using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Finbuckle.MultiTenant;

using Flow = Huybrechts.App.Features.Wiki.WikiInfoFlow;
using Finbuckle.MultiTenant.Abstractions;

namespace Huybrechts.Web.Pages.Features.Wiki;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ListQuery> _validator;
    private readonly IMultiTenantContextAccessor _multiTenantAccessor;

    public Flow.ListResult Data { get; set; } = new();

    public string TenantId { get; set; } = string.Empty;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(
        IMediator mediator, 
        IValidator<Flow.ListQuery> validator,
        IMultiTenantContextAccessor multiTenantAccessor)
    {
        _mediator = mediator;
        _validator = validator;
        _multiTenantAccessor = multiTenantAccessor;
    }

    public async Task<IActionResult> OnGetAsync(
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageIndex)
    {
        try
        {
            var message = new Flow.ListQuery()
            {
                CurrentFilter = currentFilter,
                SearchText = searchText,
                SortOrder = sortOrder,
                Page = pageIndex
            };

            ValidationResult state = await _validator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (_multiTenantAccessor.MultiTenantContext.TenantInfo is not null)
                TenantId = _multiTenantAccessor.MultiTenantContext.TenantInfo.Id ?? string.Empty;

            Data = result.Value;
            return Page();
        }
        catch(Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }
}
