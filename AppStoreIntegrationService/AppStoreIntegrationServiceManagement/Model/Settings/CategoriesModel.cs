using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class CategoriesModel
    {
        public IEnumerable<CategoryDetails> Categories { get; set; }

        public CategoryDetails NewCategory{ get; set; }
    }
}
