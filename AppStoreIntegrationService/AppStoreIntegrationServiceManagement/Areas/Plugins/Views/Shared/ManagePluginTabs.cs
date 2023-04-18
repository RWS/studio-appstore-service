using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Plugins.Views.Shared
{
    public static class ManagePluginTabs
    {
        public static string Comments => "Comments";
        public static string Logs => "Logs";
        public static string Details => "Details";
        public static string Pending => "Pending";
        public static string Draft => "Draft";
        public static string CommentsTabClass(ViewContext viewContext) => PageTabClass(viewContext, Comments, "ActiveSubPage");
        public static string LogsTabClass(ViewContext viewContext) => PageTabClass(viewContext, Logs, "ActiveSubPage");
        public static string DetailsTabClass(ViewContext viewContext) => PageTabClass(viewContext, Details, "ActiveSubPage");
        public static string PendingTabClass(ViewContext viewContext) => PageTabClass(viewContext, Pending, "ActiveSubPage");
        public static string DraftTabClass(ViewContext viewContext) => PageTabClass(viewContext, Draft, "ActiveSubPage");

        private static string PageTabClass(ViewContext viewContext, string page, string key)
        {
            var activePage = viewContext.ViewData[key] as string ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active-tab" : null;
        }
    }
}