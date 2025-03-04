using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformRateUnitFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Unit;

[Authorize(Policy = TenantPolicies.IsManager)]
public class EditModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.UpdateQuery> _getValidator;
    private readonly IValidator<Flow.UpdateCommand> _postValidator;

    [BindProperty]
    public Flow.UpdateCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public EditModel(IMediator mediator,
        IValidator<Flow.UpdateQuery> getValidator, 
        IValidator<Flow.UpdateCommand> postValidator)
    {
        _mediator = mediator;
        _getValidator = getValidator;
        _postValidator = postValidator;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync(Ulid id)
    {
        try
        {
            Flow.UpdateQuery message = new() { Id = id };

            ValidationResult state = await _getValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { platformRateId = Data.PlatformRateId });

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
            ValidationResult state = await _postValidator.ValidateAsync(Data);
            if (!state.IsValid)
            {
                state.AddToModelState(ModelState, nameof(Data) + ".");
                return Page();
            }

            var result = await _mediator.Send(Data);
            if (result.IsFailed)
            {
                result.AddToModelState(ModelState);

                var refresh = await _mediator.Send(new Flow.CreateQuery() { PlatformRateId = Data.PlatformProductId });
                Data.Platform = refresh.Value.Platform;
                Data.Product = refresh.Value.Product;
                Data.PlatformRate = refresh.Value.PlatformRate;
                Data.Rate = refresh.Value.Rate;
                Data.SetupUnits = refresh.Value.SetupUnits;

                return BadRequest(ModelState);
            }

            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index), new { platformRateId = Data.PlatformRateId });
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
