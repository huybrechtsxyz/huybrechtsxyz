using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformRateFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Rate;

[Authorize(Policy = TenantPolicies.IsMember)]
public class ImportModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ImportQuery> _validator;

    [BindProperty]
    public Flow.ImportResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public ImportModel(IMediator mediator, IValidator<Flow.ImportQuery> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<IActionResult> OnGetAsync(
        Ulid? platformProductId,
        Ulid? platformRegionId,
        Ulid? platformServiceId,
        string currencyCode,
        string currentFilter,
        string searchText,
        string sortOrder,
        int? pageIndex)
    {
        try 
        { 
            Flow.ImportQuery message = new()
            {
                PlatformProductId = platformProductId,
                PlatformRegionId = platformRegionId,
                PlatformServiceId = platformServiceId,
                CurrencyCode = currencyCode,
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

            if (result.IsFailed)
                return BadRequest();

            Data = result.Value;
            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            Flow.ImportCommand command = new()
            {
                PlatformProductId = Data.PlatformProductId ?? Ulid.Empty,
                PlatformRegionId = Data.PlatformRegionId ?? Ulid.Empty,
                PlatformServiceId = Data.PlatformServiceId ?? Ulid.Empty,
                Items = Data.Results.Where(q => q.IsSelected == true).ToList()
            };

            var result = await _mediator.Send(command);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index), new { platformProductId = Data.PlatformProductId });
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}