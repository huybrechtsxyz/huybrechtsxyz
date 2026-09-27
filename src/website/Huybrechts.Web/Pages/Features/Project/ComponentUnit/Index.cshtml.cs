using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Project.ProjectComponentUnitFlow;

namespace Huybrechts.Web.Pages.Features.Project.ComponentUnit;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ListQuery> _validator;
    private readonly IValidator<Flow.DefaultCommand> _defaultValidator;

    public Flow.ListResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(IMediator mediator, IValidator<Flow.ListQuery> validator, IValidator<Flow.DefaultCommand> defaultValidator)
    {
        _mediator = mediator;
        _validator = validator;
        _defaultValidator = defaultValidator;
    }

    public async Task<IActionResult> OnGetAsync(Ulid? ProjectComponentId)
    {
        try
        {
            Flow.ListQuery message = new()
            {
                ProjectComponentId = ProjectComponentId
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

    public async Task<IActionResult> OnGetDefaultsAsync(Ulid ProjectComponentId)
    {
        try
        {
            Flow.DefaultCommand message = new()
            {
                ProjectComponentId = ProjectComponentId
            };

            ValidationResult state = await _defaultValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return BadRequest();

            return RedirectToPage("Index", new { ProjectComponentId });
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
