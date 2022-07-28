using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AppStoreIntegrationService.Pages.Settings
{
    public static class ManageSettingsNav
    {
        public static string Index => "Index";

        public static string ImportPlugins => "Import plugins";

        public static string ExportPlugins => "ExportPlugins";

        public static string PluginsRename => "PluginRename";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string ImportPluginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ImportPlugins);

        public static string ExportPluginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExportPlugins);

        public static string PluginsRenameNavClass(ViewContext viewContext) => PageNavClass(viewContext, PluginsRename);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
