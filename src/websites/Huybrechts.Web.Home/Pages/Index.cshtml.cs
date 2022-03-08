using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Website.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
		public string PageTitle => "Welcome @ Huybrechts.xyz";

		public void OnGet()
        {

        }
    }
}