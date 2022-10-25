using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class ConfigToolModel
    {
        public List<PrivatePlugin<PluginVersion<string>>> Plugins { get; set; }
        public SelectList ProductsListItems { get; set; }
        public bool StatusExists { get; set; }
        public bool SearchExists { get; set; }
        public bool ProductExists { get; set; }
        public bool ClearAllExists { get => new[] { StatusExists, SearchExists, ProductExists }.Where(x => x).Count() >= 2; }
        public string StatusValue { get; set; }
        public string SearchValue { get; set; }
        public string ProductName { get; set; }
    }
}
