using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class PluginDetailsModel
    {
        public PrivatePlugin PrivatePlugin { get; set; }

        public List<CategoryDetails> Categories { get; set; }

        public SelectList CategoryListItems { get; set; }

        public List<int> SelectedCategories { get; set; }

        public string SelectedVersionId { get; set; }
    }
}
