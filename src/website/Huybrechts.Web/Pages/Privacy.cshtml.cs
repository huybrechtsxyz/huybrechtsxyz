using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages
{
    [AllowAnonymous]
    public class PrivacyModel : PageModel
    {
        public PrivacyModel()
        {
        }

        public void OnGet()
        {
        }
    }
}
