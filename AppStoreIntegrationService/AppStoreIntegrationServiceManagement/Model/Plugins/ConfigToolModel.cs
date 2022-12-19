using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class ConfigToolModel
    {
        public List<ExtendedPluginDetails> Plugins { get; set; }
        public SelectList ProductsListItems { get; set; }
        public SelectList StatusListItems { get; set; }
        public IEnumerable<KeyValuePair<FilterType, FilterItem>> Filters { get; set; }
    }
}
