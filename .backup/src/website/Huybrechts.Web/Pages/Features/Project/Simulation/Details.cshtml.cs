using Finbuckle.MultiTenant.Abstractions;
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
public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.DetailQuery> _getValidator;
    private readonly IValidator<Flow.CalculationQuery> _getCalcValidator;
    private readonly IValidator<Flow.DetailEntryQuery> _getEntryValidator;
    private readonly IMultiTenantContextAccessor _tenantContextAccessor;

    [BindProperty]
    public Flow.DetailResult Data { get; set; }

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public DetailsModel(IMediator mediator,
        IValidator<Flow.DetailQuery> getValidator,
        IValidator<Flow.CalculationQuery> getCalcValidator,
        IValidator<Flow.DetailEntryQuery> getEntryValidator,
        IMultiTenantContextAccessor tenantContextAccessor)
    {
        _mediator = mediator;
        _getValidator = getValidator;
        _getCalcValidator = getCalcValidator;
        _getEntryValidator = getEntryValidator;
        _tenantContextAccessor = tenantContextAccessor;
        Data = new();
    }

    public async Task<IActionResult> OnGetAsync(Ulid id)
    {
        try
        {
            Flow.DetailQuery message = new() { Id = id };

            ValidationResult state = await _getValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { Data.ProjectInfoId });

            Data = result.Value;
            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }

    public async Task<IActionResult> OnGetCalculateAsync(Ulid id)
    {
        try
        {
            var tenantInfo = _tenantContextAccessor.MultiTenantContext.TenantInfo;
            if (tenantInfo is null || string.IsNullOrEmpty(tenantInfo.Id))
                return BadRequest();

            Flow.CalculationQuery message = new() { TenantId = tenantInfo.Id, ProjectSimulationId = id };

            ValidationResult state = await _getCalcValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { Data.ProjectInfoId });

            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetEntriesAsync(Ulid projectSimulationId)
    {
        try
        {
            Flow.DetailEntryQuery message = new() { Id = projectSimulationId };

            ValidationResult state = await _getEntryValidator.ValidateAsync(message);
            if (!state.IsValid)
                return BadRequest(state);

            var result = await _mediator.Send(message) ?? new();
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            if (result.IsFailed)
                return RedirectToPage(nameof(Index), new { Data.ProjectInfoId });

            Data = result.Value;
            return new JsonResult(Data.SimulationEntries);
            
        }
        catch (Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
