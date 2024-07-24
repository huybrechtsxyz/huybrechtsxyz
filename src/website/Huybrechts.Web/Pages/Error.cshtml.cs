using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Huybrechts.Web.Pages
{
    public class ErrorModel : PageModel
    {
        public string? ErrorCode { get; set; }

        public string? ErrorText { get; set; }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet(int status)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorCode = status.ToString();
            ErrorText = Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(status);
        }
    }
}
