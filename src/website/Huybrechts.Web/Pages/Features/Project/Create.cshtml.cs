using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Setup.SetupNoSerieFlow;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Flow = Huybrechts.App.Features.Project.ProjectInfoFlow;

namespace Huybrechts.Web.Pages.Features.Project;

[Authorize(Policy = TenantPolicies.IsContributor)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.CreateQuery> _getValidator;
    private readonly IValidator<Flow.CreateCommand> _postValidator;
    private readonly IValidator<NoSerieQuery> _codeValidator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public CreateModel(IMediator mediator,
        IValidator<Flow.CreateQuery> getValidator,
        IValidator<Flow.CreateCommand> postValidator,
        IValidator<NoSerieQuery> codeValidator)
    {
        _mediator = mediator;
        _getValidator = getValidator;
        _postValidator = postValidator;
        _codeValidator = codeValidator;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Flow.CreateQuery message = new() { };

            ValidationResult state = await _getValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index));

            Data = result.Value;

            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }

    public async Task<JsonResult> OnGetProjectNoAsync(string projectType, DateTime? dateTime)
    {
        NoSerieQuery noSerieQuery = new()
        {
            TypeOf = SetupNoSerieHelper.PROJECTCODE,
            TypeValue = projectType,
            DateTime = dateTime ?? DateTime.Today,
            DoPeek = true
        };

        ValidationResult state = await _codeValidator.ValidateAsync(noSerieQuery);
        if (!state.IsValid)
            return new JsonResult(state.AsResult());

        var result = await _mediator.Send(noSerieQuery);
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();

        return new JsonResult(result);
    }

    public async Task<JsonResult> OnGetSubcategoriesAsync(string category)
    {
        Flow.CreateQuery message = new() { };
        var result = await _mediator.Send(message) ?? new();
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();
        if (result.IsFailed)
            return new JsonResult(null);
        return new JsonResult(result.Value.LoadSubcategories(category));
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

            return RedirectToPage(nameof(Index));
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }
}
