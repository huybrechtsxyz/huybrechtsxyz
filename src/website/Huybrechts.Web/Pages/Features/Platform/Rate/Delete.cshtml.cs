using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using Huybrechts.Core.Platform;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Platform.PlatformRateFlow;

namespace Huybrechts.Web.Pages.Features.Platform.Rate;

[Authorize(Policy = TenantPolicies.IsManager)]
public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.DeleteQuery> _getValidator;
    private readonly IValidator<Flow.DeleteCommand> _postValidator;

    [BindProperty]
    public Flow.DeleteCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DeleteModel(IMediator mediator,
        IValidator<Flow.DeleteQuery> getValidator,
        IValidator<Flow.DeleteCommand> postValidator)
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
            Flow.DeleteQuery message = new() { Id = id };

            ValidationResult state = await _getValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { platformInfoId = Data.PlatformInfoId });

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

                var refresh = await _mediator.Send(new Flow.CreateQuery() { PlatformProductId = Data.PlatformProductId });
                Data.Platform = refresh.Value.Platform;
                Data.Product = refresh.Value.Product;
                Data.Regions = refresh.Value.Regions;
                Data.Services = refresh.Value.Services;
                Data.Currencies = refresh.Value.Currencies;

                return Page();
            }

            var result = await _mediator.Send(Data);
            if (result.IsFailed)
            {
                result.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }

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
