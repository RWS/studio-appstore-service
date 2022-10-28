using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ProductsSynchronizer : IProductsSynchronizer
    {
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public ProductsSynchronizer(IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        public bool ExistDuplicate(IEnumerable<ParentProduct> products)
        {
            return !products.GroupBy(p => p.ParentProductName, (_, products) => products
                            .Count() == 1)
                            .All(item => item);
        }

        public bool ExistDuplicate(IEnumerable<ProductDetails> products)
        {
            return !products.GroupBy(p => p.ProductName, (_, products) => products
                            .GroupBy(p => p.MinimumStudioVersion, (_, products) => products
                            .Count() == 1).All(item => item))
                            .All(item => item);
        }

        public async Task<bool> IsInUse(string id, ProductType type)
        {
            var plugins = await _pluginRepository.GetAll(null);
            var products = await _productsRepository.GetAllProducts();
            return type switch
            {
                ProductType.Child => plugins.Select(p => p.Versions.Any(v => v.SupportedProducts[0] == id)).Any(item => item),
                _ => products.Any(p => p.ParentProductID.ToString() == id)
            };
        }

        public string SetIndex(IEnumerable<ProductDetails> products)
        {
            var lastProduct = products.LastOrDefault();
            if (lastProduct == null)
            {
                return "1";
            }

            return (int.Parse(lastProduct.Id) + 1).ToString();
        }

        public string SetIndex(IEnumerable<ParentProduct> products)
        {
            var lastProduct = products.LastOrDefault();
            if (lastProduct == null)
            {
                return "1";
            }

            return (int.Parse(lastProduct.ParentId) + 1).ToString();
        }
    }
}
