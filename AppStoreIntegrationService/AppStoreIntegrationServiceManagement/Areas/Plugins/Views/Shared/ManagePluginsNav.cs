using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Plugins.Views.Shared
{
    public static class ManagePluginsNav
    {
        public static string PluginDetails => "PluginDetails";
        public static string Versions => "Versions";
        public static string Reviews => "Reviews";
        public static string Comments => "Comments";
        public static string Logs => "Logs";
        public static string Details => "Details";
        public static string PluginDetailsNavClass(ViewContext viewContext) => PageNavClass(viewContext, PluginDetails, "ActivePage", "active");
        public static string VersionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Versions, "ActivePage", "active");
        public static string ReviewsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Reviews, "ActivePage", "active");
        public static string CommentsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Comments, "ActiveSubPage", "active-tab");
        public static string LogsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Logs, "ActiveSubPage", "active-tab");
        public static string DetailsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Details, "ActiveSubPage", "active-tab");

        private static string PageNavClass(ViewContext viewContext, string page, string key, string className)
        {
            var activePage = viewContext.ViewData[key] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? className : null;
        }
}
}
