using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Setup.SetupUnitFlow;

namespace Huybrechts.Web.Pages.Features.Setup.Unit;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ListQuery> _validator;
    private readonly ILogger<IndexModel> _logger;

    public Flow.ListResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(IMediator mediator, IValidator<Flow.ListQuery> validator, ILogger<IndexModel> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(
        string? currentFilter,
        string? searchText,
        string? sortOrder,
        int? pageIndex)
    {
        try
        {
            var request = new Flow.ListQuery()
            {
                CurrentFilter = currentFilter ?? string.Empty,
                SearchText = searchText ?? string.Empty,
                SortOrder = sortOrder ?? string.Empty,
                Page = pageIndex
            };

            ValidationResult validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                return Page();
            }

            var response = await _mediator.Send(request);
            if (response.HasStatusMessage())
                StatusMessage = response.ToStatusMessage();

            Data = response.Value;
            return Page();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
