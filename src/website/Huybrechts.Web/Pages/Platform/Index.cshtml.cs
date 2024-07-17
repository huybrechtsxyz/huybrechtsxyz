using Huybrechts.Core.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Platform
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IList<PlatformInfo> Records { get; set; } = [];

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public IndexModel()
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
    }
}
