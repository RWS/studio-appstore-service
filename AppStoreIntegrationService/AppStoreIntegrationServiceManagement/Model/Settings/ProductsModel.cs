using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ProductsModel
    {
        public IEnumerable<SupportedProductDetails> Products { get; set; }

        public SupportedProductDetails NewProduct { get; set; }
    }
}
