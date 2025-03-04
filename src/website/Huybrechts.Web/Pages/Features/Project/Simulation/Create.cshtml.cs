using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Project.ProjectSimulationFlow;

namespace Huybrechts.Web.Pages.Features.Project.Simulation;

[Authorize(Policy = TenantPolicies.IsContributor)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.CreateQuery> _getValidator;
    private readonly IValidator<Flow.CreateCommand> _postValidator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator, 
        IValidator<Flow.CreateQuery> getValidator, 
        IValidator<Flow.CreateCommand> postValidator)
    {
        _mediator = mediator;
        Data = new();
        _getValidator = getValidator;
        _postValidator = postValidator;
    }

    public async Task<IActionResult> OnGetAsync(Ulid ProjectInfoId)
    {
        try
        {
            Flow.CreateQuery message = new() { ProjectInfoId = ProjectInfoId };

            ValidationResult state = await _getValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { ProjectInfoId = ProjectInfoId });

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
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index), new { ProjectInfoId = Data.ProjectInfoId });
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
