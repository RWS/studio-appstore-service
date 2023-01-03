using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ProductsModel
    {
        public IEnumerable<ExtendedProductDetails> Products { get; set; }

        public IEnumerable<ParentProduct> ParentProducts { get; set; }
    }
}
