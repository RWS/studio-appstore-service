using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class ProductsSynchronizer : IProductsSynchronizer
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> _pluginRepositoryExtended;
        private readonly IProductsRepository _productsRepository;

        public ProductsSynchronizer(IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepositoryExtended = pluginRepository;
            _productsRepository = productsRepository;
        }

        public async Task<bool> IsInUse(string id)
        {
            var plugins = await _pluginRepositoryExtended.GetAll(null);
            return plugins.Select(p => p.Versions
                          .Any(v => v.SupportedProducts[0] == id))
                          .Any(item => item);
        }

        public async Task SyncOnUpdate(List<ProductDetails> products)
        {
            var plugins = await _pluginRepositoryExtended.GetAll(null);
            await SyncPluginsAndProducts(plugins, products);
        }

        private async Task SyncPluginsAndProducts(List<PluginDetails<PluginVersion<string>>> plugins, List<ProductDetails> products)
        {
            foreach (var version in plugins.SelectMany(p => p.Versions))
            {
                foreach (var product in products)
                {
                    if (product.Id == version.SupportedProducts[0])
                    {
                        version.SupportedProducts = new List<string> { product.Id };
                    }
                }
            }

            await _pluginRepositoryExtended.SaveToFile(plugins);
        }
    }
}
