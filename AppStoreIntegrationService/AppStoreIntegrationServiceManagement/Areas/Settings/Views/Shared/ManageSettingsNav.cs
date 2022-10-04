using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Settings.Views.Shared
{
    public static class ManageSettingsNav
    {
        public static string Index => "Index";

        public static string ImportPlugins => "Import plugins";

        public static string ExportPlugins => "ExportPlugins";

        public static string PluginsRename => "PluginRename";

        public static string Products => "Products";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string ImportPluginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ImportPlugins);

        public static string ExportPluginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExportPlugins);

        public static string PluginsRenameNavClass(ViewContext viewContext) => PageNavClass(viewContext, PluginsRename);

        public static string ProductsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Products);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
