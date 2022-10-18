using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class ProductsRepository : IProductsRepository
    {
        protected readonly IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> _azureRepositoryExtended;
        protected readonly IConfigurationSettings _configurationSettings;
        protected readonly List<ProductDetails> _defaultProducts;

        public ProductsRepository(IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> azureRepositoryExtended, IConfigurationSettings configurationSettings)
        {
            _azureRepositoryExtended = azureRepositoryExtended;
            _configurationSettings = configurationSettings;
            _defaultProducts = new List<ProductDetails>
            {
                new ProductDetails
                {
                    Id = "37",
                    ProductName = "SDL Trados Studio 2021",
                    ParentProductID = 14,
                    MinimumStudioVersion = "16.0"
                },
                new ProductDetails
                {
                    Id = "38",
                    ProductName = "Trados Studio 2022",
                    ParentProductID = 14,
                    MinimumStudioVersion = "17.0"
                },

            };
        }

        public async Task<List<ProductDetails>> ReadLocalProducts(string productsFilePath)
        {
            var products = await File.ReadAllTextAsync(productsFilePath);
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>>>>(products).Products ?? new List<ProductDetails>();
        }

        public async Task UpdateProducts(List<ProductDetails> products)
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                await File.WriteAllTextAsync(_configurationSettings.ProductsFilePath, JsonConvert.SerializeObject(products));
                return;
            }

            await _azureRepositoryExtended.UpdateProductsFileBlob(JsonConvert.SerializeObject(products));
        }

        private async Task<List<ProductDetails>> GetProductsFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                if (string.IsNullOrEmpty(_configurationSettings.ProductsFilePath))
                {
                    return new List<ProductDetails>();
                }

                return await ReadLocalProducts(_configurationSettings.ProductsFilePath);
            }

            return await _azureRepositoryExtended.GetProductsFromContainer();
        }

        public async Task DeleteProduct(string id)
        {
            var newProducts = (await GetProductsFromPossibleLocation()).Where(item => item.Id != id).ToList();
            await UpdateProducts(newProducts);
        }

        public async Task<IEnumerable<ProductDetails>> GetAllProducts()
        {
            var products = await GetProductsFromPossibleLocation();
            return products.Any() ? products : _defaultProducts;
        }
    }
}
