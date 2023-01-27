using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Plugins.Views.Shared
{
    public static class ManagePluginsNav
    {
        public static string PluginDetails => "PluginDetails";
        public static string Versions => "Versions";
        public static string PluginDetailsNavClass(ViewContext viewContext) => PageNavClass(viewContext, PluginDetails, "ActivePage", "active");
        public static string VersionsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Versions, "ActivePage", "active");

        private static string PageNavClass(ViewContext viewContext, string page, string key, string className)
        {
            var activePage = viewContext.ViewData[key] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? className : null;
        }
    }
}
