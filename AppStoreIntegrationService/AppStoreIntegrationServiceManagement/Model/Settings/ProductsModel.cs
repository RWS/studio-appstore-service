using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class ProductsModel
    {
        public IEnumerable<ExtendedProductDetails> Products { get; set; }

        public ExtendedProductDetails NewProduct { get; set; }

        public IEnumerable<ParentProduct> ParentProducts { get; set; }

        public ParentProduct NewParentProduct { get; set; }
    }
}
