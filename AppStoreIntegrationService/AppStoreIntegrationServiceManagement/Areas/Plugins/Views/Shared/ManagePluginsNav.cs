using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Plugins.Views.Shared
{
    public static class ManagePluginsNav
    {
        public static string Details => "Details";
        public static string Versions => "Versions";
        public static string Comments => "Comments";
        public static string DetailsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Details);
        public static string VersionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Versions);
        public static string CommentsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Comments);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
