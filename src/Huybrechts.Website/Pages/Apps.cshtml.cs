using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Website.Pages
{
    public class AppsModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public AppsModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }

}
