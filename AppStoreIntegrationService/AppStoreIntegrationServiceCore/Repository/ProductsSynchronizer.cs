using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ProductsSynchronizer : IProductsSynchronizer
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private int minId = -1;

        public ProductsSynchronizer(IPluginRepository pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        public async Task<bool> IsInUse(string id)
        {
            var plugins = await _pluginRepository.GetAll(null);
            return plugins.Select(p => p.Versions
                          .Any(v => v.SupportedProducts
                          .FirstOrDefault().Id == id))
                          .Any(item => item);
        }

        public async Task Sync()
        {
            var plugins = await _pluginRepository.GetAll(null);
            var products = await _productsRepository.GetAllProducts();
            await SyncPluginsAndProducts(plugins, products.ToList());
        }

        public async Task SyncOnImport(List<PluginDetails> plugins)
        {
            var products = plugins.SelectMany(p => p.Versions
                                  .Select(v => v.SupportedProducts[0]))
                                  .Distinct().ToList();

            foreach (var item in products)
            {
                EnsureNonDuplicateId(item, plugins);
            }

            await _productsRepository.UpdateProducts(products);
            await _pluginRepository.SaveToFile(plugins);
            minId = -1;
        }

        public async Task SyncOnUpdate(List<SupportedProductDetails> products)
        {
            var plugins = await _pluginRepository.GetAll(null);
            await SyncPluginsAndProducts(plugins, products);
        }

        private void EnsureNonDuplicateId(SupportedProductDetails product, List<PluginDetails> plugins)
        {
            minId = minId == -1 ? int.Parse(product.Id) : minId + 1;
            product.Id = minId.ToString();
            foreach (var version in plugins.SelectMany(p => p.Versions))
            {
                if (product.ProductName == version.SupportedProducts[0].ProductName)
                {
                    version.SupportedProducts = new List<SupportedProductDetails> { product };
                }
            }
        }

        private async Task SyncPluginsAndProducts(List<PluginDetails> plugins, List<SupportedProductDetails> products)
        {
            foreach (var version in plugins.SelectMany(p => p.Versions))
            {
                foreach (var product in products)
                {
                    if (product.Id == version.SupportedProducts[0].Id)
                    {
                        version.SupportedProducts = new List<SupportedProductDetails> { product };
                    }
                }
            }

            await _pluginRepository.SaveToFile(plugins);
        }
    }
}
