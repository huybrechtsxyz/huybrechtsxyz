using Finbuckle.MultiTenant.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using Huybrechts.App.Features.Wiki.WikiInfoFlow;
using Huybrechts.App.Web;
using Markdig;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Flow = Huybrechts.App.Features.Wiki.WikiInfoFlow;

namespace Huybrechts.Web.Pages.Features.Wiki;

[Authorize(Policy = TenantPolicies.IsMember)]
public class WikiPageModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IValidator<Flow.EditQuery> _qryValidator;
    private readonly IValidator<Flow.EditCommand> _cmdValidator;
    private readonly IValidator<Flow.DeleteCommand> _delValidator;
    private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;

    [BindProperty(SupportsGet = true)]
    public string WikiSpace { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string WikiPage { get; set; } = string.Empty;

    [BindProperty]
    public Flow.EditCommand Data { get; set; } = new();

    public string HtmlContent { get; set; } = string.Empty;

    public bool IsEditable { get; set; } = false;
    
    public bool IsDeletable { get; set; } = false;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public WikiPageModel(
        IMediator mediator,
        IValidator<Flow.EditQuery> qryValidator,
        IValidator<Flow.EditCommand> cmdValidator,
        IValidator<Flow.DeleteCommand> delValidator,
        IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        _mediator = mediator;
        _qryValidator = qryValidator;
        _cmdValidator = cmdValidator;
        _delValidator = delValidator; 
        _multiTenantContextAccessor = multiTenantContextAccessor;
    }

    public async Task<IActionResult> OnGetAsync(bool edit = false, bool delete=false)
    {
        try
        {
            if (string.IsNullOrEmpty(this.WikiPage))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(this.WikiSpace))
            {
                this.WikiSpace = "wiki";
            }

            IsEditable = edit;
            IsDeletable = delete;
            Flow.EditQuery message = new()
            {
                Namespace = this.WikiSpace,
                Page = this.WikiPage
            };

            ValidationResult state = await _qryValidator.ValidateAsync(message);
            if (!state.IsValid)
            {
                state.AddToModelState(ModelState);
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(message);
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            Data = result.Value;
            SetPageContent();
            return Page();
        }
        catch(Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        try
        {
            ValidationResult state = await _cmdValidator.ValidateAsync(Data);
            if (!state.IsValid)
            {
                state.AddToModelState(ModelState);
                return Page();
            }

            var result = await _mediator.Send(Data);
            if (result.IsFailed)
            {
                result.AddToModelState(ModelState);
                return Page();
            }
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            SetPageContent();
            return Page();
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(this.WikiPage))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(this.WikiSpace))
            {
                this.WikiSpace = "wiki";
            }

            Flow.DeleteCommand message = new()
            {
                Namespace = this.WikiSpace,
                Page = this.WikiPage
            };

            ValidationResult state = await _delValidator.ValidateAsync(message);
            if (!state.IsValid)
            {
                state.AddToModelState(ModelState);
                return Page();
            }

            var result = await _mediator.Send(message);
            if (result.IsFailed)
            {
                result.AddToModelState(ModelState);
                return Page();
            }
            if (result.HasStatusMessage())
                StatusMessage = result.ToStatusMessage();

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError, message = ex.Message });
        }
    }

    private void SetPageContent()
    {
        var tenant = _multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Id;
        var pipeline = new MarkdownPipelineBuilder()
            .Use(new WikiLinkExtension(
                WikiInfoHelper.GetUrlPrefix(tenant ?? string.Empty),
                WikiInfoHelper.GetDefaultNamespace()))
            .UseAdvancedExtensions()
            .Build();
        this.HtmlContent = Markdown.ToHtml(Data.Content ?? string.Empty, pipeline);
    }
}
