using Huybrechts.App.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Features.Setup;

[Authorize(Policy = TenantPolicies.IsManager)]
public class IndexModel : PageModel
{
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IndexModel()
    { 
    }

    public IActionResult OnGet()
    {
        try
        {
            return Page();
        }
        catch(Exception)
        {
            return RedirectToPage("/Error", new { status = StatusCodes.Status500InternalServerError });
        }
    }
}
