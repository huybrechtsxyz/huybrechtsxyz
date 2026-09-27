using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Setup.SetupCategoryFlow;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Flow = Huybrechts.App.Features.Setup.SetupCategoryFlow;

namespace Huybrechts.Web.Pages.Features.Setup.Category;

[Authorize(Policy = TenantPolicies.IsManager)]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.CreateCommand> _validator;

    [BindProperty]
    public Flow.CreateCommand Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public SelectList TypeOfOptions { get; init; }

    public CreateModel(IMediator mediator, IValidator<Flow.CreateCommand> validator)
    {
        _mediator = mediator;
        _validator = validator;
        TypeOfOptions = new SelectList(SetupCategoryHelper.AllowedTypeOfValues);
        Data = new();
    }

    public void OnGet()
    {
        Data = Flow.SetupCategoryHelper.CreateNew();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            ValidationResult state = await _validator.ValidateAsync(Data);
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
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
