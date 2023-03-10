using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class ConfigToolModel
    {
        public IEnumerable<ExtendedPluginDetails> Plugins { get; set; }
        public SelectList ProductsListItems { get; set; }
        public SelectList StatusListItems { get; set; }
        public IEnumerable<FilterItem> Filters { get; set; }
    }
}
