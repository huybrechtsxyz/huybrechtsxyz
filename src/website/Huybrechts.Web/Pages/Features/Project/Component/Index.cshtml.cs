using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Project.ProjectComponentFlow;

namespace Huybrechts.Web.Pages.Features.Project.Component;

[Authorize(Policy = TenantPolicies.IsMember)]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ListQuery> _validator;

    public Flow.ListResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel(IMediator mediator, IValidator<Flow.ListQuery> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<IActionResult> OnGetAsync(
        Ulid? ProjectDesignId,
        Ulid? parentId)
    {
        try
        {
            Flow.ListQuery message = new()
            {
                ProjectDesignId = ProjectDesignId,
                ParentId = parentId,
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
        catch (Exception ex)
        {
            return RedirectToPage("/Error",
                new
                {
                    status = StatusCodes.Status500InternalServerError,
                    message = ex.Message
                });
        }
    }

    public async Task<IActionResult> OnGetLoadAsync(
        Ulid? ProjectDesignId,
        Ulid? parentId)
    {
        try
        {
            Flow.ListQuery message = new()
            {
                ProjectDesignId = ProjectDesignId,
                ParentId = parentId,
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
            return new JsonResult(Data.Results);
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error",
                new
                {
                    status = StatusCodes.Status500InternalServerError,
                    message = ex.Message
                });
        }
        
    }
}
