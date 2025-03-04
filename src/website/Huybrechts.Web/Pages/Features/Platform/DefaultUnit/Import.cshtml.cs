using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Web;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Platform.PlatformDefaultUnitFlow;

namespace Huybrechts.Web.Pages.Features.Platform.DefaultUnit;

[Authorize(Policy = TenantPolicies.IsManager)]
public class ImportModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.ImportQuery> _getValidator;
    private readonly IValidator<Flow.ImportCommand> _postValidator;

    [BindProperty]
    public Flow.ImportResult Data { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public ImportModel(IMediator mediator, 
        IValidator<Flow.ImportQuery> getValidator, 
        IValidator<Flow.ImportCommand> postValidator)
    {
        _mediator = mediator;
        _getValidator = getValidator;
        _postValidator = postValidator;
    }

    public async Task<IActionResult> OnGetAsync(
        Ulid? platformInfoId,
        string? currentFilter,
        string? searchText,
        string? sortOrder,
        int? pageIndex)
    {
        var message = new Flow.ImportQuery()
        {
            PlatformInfoId = platformInfoId ?? Ulid.Empty,
            CurrentFilter = currentFilter ?? string.Empty,
            SearchText = searchText ?? string.Empty,
            SortOrder = sortOrder ?? string.Empty,
            Page = pageIndex
        };
        
        ValidationResult state = await _getValidator.ValidateAsync(message);
        if (!state.IsValid)
            return RedirectToPage(nameof(Index), new { platformInfoId = Data.PlatformInfo.Id });

        var result = await _mediator.Send(message);
        if (result.HasStatusMessage())
            StatusMessage = result.ToStatusMessage();

        Data = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var selection = Data.Results.Where(q => q.IsSelected == true).ToList();
            Flow.ImportCommand message = new()
            {
                PlatformInfoId = Data.PlatformInfo.Id,
                Items = selection
            };

            ValidationResult state = await _postValidator.ValidateAsync(message);
            if (!state.IsValid)
            {
                var refresh = await _mediator.Send(new Flow.ImportQuery() { PlatformInfoId = Data.PlatformInfo.Id });
                Data.PlatformInfo = refresh.Value.PlatformInfo;
                Data.SetupUnits = refresh.Value.SetupUnits;
                state.AddToModelState(ModelState, nameof(Data) + ".");
                return Page();
            }

            Result result = await _mediator.Send(message);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage(nameof(Index), new { platformInfoId = Data.PlatformInfo.Id });
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, mesage = ex.Message });
        }
    }
}