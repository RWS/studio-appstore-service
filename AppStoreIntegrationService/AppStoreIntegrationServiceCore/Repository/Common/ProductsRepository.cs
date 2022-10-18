using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public enum ProductType
    {
        Child = 0,
        Parent
    }

    public class ProductsRepository : IProductsRepository
    {
        protected readonly IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> _azureRepositoryExtended;
        protected readonly IConfigurationSettings _configurationSettings;
        protected readonly List<ProductDetails> _defaultProducts;
        protected readonly List<ParentProduct> _defaultParentProducts;

        public ProductsRepository(IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> azureRepositoryExtended, IConfigurationSettings configurationSettings)
        {
            _azureRepositoryExtended = azureRepositoryExtended;
            _configurationSettings = configurationSettings;
            _defaultProducts = new List<ProductDetails>
            {
                new ProductDetails
                {
                    Id = "1",
                    ProductName = "SDL Trados Studio 2021",
                    ParentProductID = "14",
                    MinimumStudioVersion = "16.0"
                },
                new ProductDetails
                {
                    Id = "2",
                    ProductName = "Trados Studio 2022",
                    ParentProductID = "14",
                    MinimumStudioVersion = "17.0"
                },

            };

            _defaultParentProducts = new List<ParentProduct>()
            {
                new ParentProduct
                {
                    ParentId = "1",
                    ParentProductName = "Trados Studio"
                }
            };
        }

        public async Task UpdateProducts(List<ProductDetails> products)
        {
            var response = await ReadLocalFile();
            var newResponse = new PluginResponse<PluginDetails<PluginVersion<string>>>
            {
                Value = response.Value,
                Products = products,
                ParentProducts = response.ParentProducts
            };

            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, JsonConvert.SerializeObject(newResponse));
                return;
            }

            await _azureRepositoryExtended.UpdateProductsFileBlob(JsonConvert.SerializeObject(newResponse));
        }

        private async Task<PluginResponse<PluginDetails<PluginVersion<string>>>> ReadLocalFile()
        {
            var fileContent = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePathV2);
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>>>>(fileContent) ?? new PluginResponse<PluginDetails<PluginVersion<string>>>();
        }

        private async Task<List<ProductDetails>> GetProductsFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePathV2))
                {
                    return new List<ProductDetails>();
                }

                return (await ReadLocalFile()).Products ?? new List<ProductDetails>();
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

        public async Task<IEnumerable<ParentProduct>> GetAllParentProducts()
        {
            var products = await GetParentProductsFromPossibleLocation();
            return products.Any() ? products : _defaultParentProducts;
        }

        private async Task<List<ParentProduct>> GetParentProductsFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePathV2))
                {
                    return new List<ParentProduct>();
                }

                return (await ReadLocalFile()).ParentProducts ?? new List<ParentProduct>();
            }

            return await _azureRepositoryExtended.GetParentProductsFromContainer();
        }

        public async Task UpdateParentProducts(List<ParentProduct> products)
        {
            var response = await ReadLocalFile();
            var newResponse = new PluginResponse<PluginDetails<PluginVersion<string>>>
            {
                Value = response.Value,
                Products = response.Products,
                ParentProducts = products
            };

            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, JsonConvert.SerializeObject(newResponse));
                return;
            }

            await _azureRepositoryExtended.UpdateProductsFileBlob(JsonConvert.SerializeObject(newResponse));
        }

        public async Task DeleteParentProduct(string id)
        {
            var newProducts = (await GetParentProductsFromPossibleLocation()).Where(item => item.ParentId != id).ToList();
            await UpdateParentProducts(newProducts);
        }
    }
}
