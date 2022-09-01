﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Identity.Views.Shared
{
    public static class ManageNavPages
    {
        public static string Index => "Index";

        public static string Register => "Register";

        public static string ChangePassword => "ChangePassword";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string RegisterNavClass(ViewContext viewContext) => PageNavClass(viewContext, Register);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
