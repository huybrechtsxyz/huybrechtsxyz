// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.Rendering;

namespace  Huybrechts.Web.Pages.Features.Setup
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ManageNavPages
    {
        public static string Index => "Index";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string Unit => "Unit";

        public static string UnitNavClass(ViewContext viewContext) => PageNavClass(viewContext, Unit);

        public static string Language => "Language";

        public static string LanguageNavClass(ViewContext viewContext) => PageNavClass(viewContext, Language);

        public static string Country => "Country";

        public static string CountryNavClass(ViewContext viewContext) => PageNavClass(viewContext, Country);

        public static string Currency => "Currency";

        public static string CurrencyNavClass(ViewContext viewContext) => PageNavClass(viewContext, Currency);

        public static string State => "State";

        public static string StateNavClass(ViewContext viewContext) => PageNavClass(viewContext, State);

        public static string Type => "Type";

        public static string TypeNavClass(ViewContext viewContext) => PageNavClass(viewContext, Type);

        public static string Category => "Category";

        public static string CategoryNavClass(ViewContext viewContext) => PageNavClass(viewContext, Category);
        
        public static string NoSerie => "NoSerie";

        public static string NoSerieNavClass(ViewContext viewContext) => PageNavClass(viewContext, NoSerie);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
