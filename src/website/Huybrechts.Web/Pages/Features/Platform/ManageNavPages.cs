using Microsoft.AspNetCore.Mvc.Rendering;

namespace Huybrechts.Web.Pages.Features.Platform;

public static class ManageNavPages
{
    public static string Info => "Info";

    public static string InfoNavClass(ViewContext viewContext) => PageNavClass(viewContext, Info);

    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : string.Empty;
    }
}
