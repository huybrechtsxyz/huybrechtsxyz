using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Project.ProjectComponentFlow;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Project.ProjectComponentFlow;

namespace Huybrechts.Web.Pages.Features.Project.Component;

[Authorize(Policy = TenantPolicies.IsContributor)]
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
                return RedirectToPage(nameof(Index), new { Data.ProjectDesignId });

            Data = result.Value;
            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }

    public async Task<IActionResult> OnGetFieldValuesAsync(Ulid projectInfoId, string propertyName)
    {
        DistinctFieldQuery query = new() { ProjectInfoId = projectInfoId, FieldName = propertyName };
        var result = await _mediator.Send(query);
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();
        return new JsonResult(result.Value);
    }

    public async Task<IActionResult> OnGetPlatformsAsync()
    {
        Huybrechts.App.Features.Platform.PlatformInfoFlow.ListQuery listQuery = new() { };
        var result = await _mediator.Send(listQuery);
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();
        return new JsonResult(result.Value.Results);
    }

    public async Task<IActionResult> OnGetProductsAsync(Ulid platformInfoId)
    {
        Huybrechts.App.Features.Platform.PlatformProductFlow.ListQuery listQuery = new() { PlatformInfoId = platformInfoId };
        var result = await _mediator.Send(listQuery);
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();
        return new JsonResult(result.Value.Results);
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
                return BadRequest(ModelState);
            }

            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index), new { Data.ProjectDesignId });
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
