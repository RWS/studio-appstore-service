using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class PluginDetailsModel
    {
        public PrivatePlugin<PluginVersion<string>> PrivatePlugin { get; set; }

        public List<CategoryDetails> Categories { get; set; }

        public MultiSelectList CategoryListItems { get; set; }

        public List<int> SelectedCategories { get; set; } = new List<int>();

        public string SelectedVersionId { get; set; }
    }
}
