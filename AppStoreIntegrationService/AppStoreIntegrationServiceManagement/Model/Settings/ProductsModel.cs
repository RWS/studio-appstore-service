using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ProductsModel
    {
        public IEnumerable<ProductDetails> Products { get; set; }

        public ProductDetails NewProduct { get; set; }
    }
}
