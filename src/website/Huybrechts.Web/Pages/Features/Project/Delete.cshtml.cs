using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Project.ProjectInfoFlow;

namespace Huybrechts.Web.Pages.Features.Project;

[Authorize(Policy = TenantPolicies.IsManager)]
public class DeleteModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.DeleteQuery> _getValidator;
    private readonly IValidator<Flow.DeleteCommand> _postValidator;

    [BindProperty]
    public Flow.DeleteCommand Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DeleteModel(IMediator mediator,
        IValidator<Flow.DeleteQuery> getValidator, 
        IValidator<Flow.DeleteCommand> postValidator)
    {
        _mediator = mediator;
        _getValidator = getValidator;
        _postValidator = postValidator;
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
                return RedirectToPage(nameof(Index));

            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

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
                state.AddToModelState(this.ModelState);
                return Page();
            }

            var result = await _mediator.Send(Data);
            if (result.IsFailed)
            {
                state.AddToModelState(this.ModelState);
                return BadRequest(ModelState);
            }

            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index));
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
