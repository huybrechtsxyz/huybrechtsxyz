using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Huybrechts.Web.Pages
{
    public class ErrorModel : PageModel
    {
        public string ErrorCode { get; set; }

        public string ErrorText { get; set; }

        public string ErrorStyle => ErrorCode[..1] switch
        {
            "4" => "bg-warning",
            _ => "bg-danger"
        };

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public ErrorModel()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorCode = "400";
            ErrorText = Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(400);
        }

        public void OnGet(int status, string message = "")
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorCode = status.ToString();
            if (string.IsNullOrEmpty(message))
                ErrorText = Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(status); 
            else
                ErrorText = message;
        }
    }
}
