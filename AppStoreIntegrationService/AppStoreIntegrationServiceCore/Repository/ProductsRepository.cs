using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IAzureRepository _azureRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public ProductsRepository(IAzureRepository azureRepository, IConfigurationSettings configurationSettings)
        {
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
        }

        public async Task<IEnumerable<SupportedProductDetails>> GetAllProducts()
        {
            var defaultProducts = new List<SupportedProductDetails>
            {
                new SupportedProductDetails
                {
                    Id = "37",
                    ProductName = "SDL Trados Studio 2021",
                    ParentProductID = 14,
                    MinimumStudioVersion = "16.0"
                },
                new SupportedProductDetails
                {
                    Id = "38",
                    ProductName = "Trados Studio 2022",
                    ParentProductID = 14,
                    MinimumStudioVersion = "17.0"
                },
                
            };

            var products = await GetProductsFromPossibleLocation();
            if (products.Any())
            {
                return products;
            }

            await UpdateProducts(defaultProducts);
            return defaultProducts;
        }

        private async Task<List<SupportedProductDetails>> GetProductsFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                if (string.IsNullOrEmpty(_configurationSettings.ProductsFilePath))
                {
                    return new List<SupportedProductDetails>();
                }

                return await ReadLocalProducts(_configurationSettings.ProductsFilePath);
            }

            return await _azureRepository.GetProductsFromContainer();
        }

        public async Task<List<SupportedProductDetails>> ReadLocalProducts(string productsFilePath)
        {
            var products = await File.ReadAllTextAsync(productsFilePath);
            return JsonConvert.DeserializeObject<List<SupportedProductDetails>>(products) ?? new List<SupportedProductDetails>();
        }

        public async Task UpdateProducts(List<SupportedProductDetails> products)
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                await File.WriteAllTextAsync(_configurationSettings.ProductsFilePath, JsonConvert.SerializeObject(products));
                return;
            }

            await _azureRepository.UpdateProductsFileBlob(JsonConvert.SerializeObject(products));
        }

        public async Task DeleteProduct(string id)
        {
            var newProducts = (await GetProductsFromPossibleLocation()).Where(item => item.Id != id).ToList();
            await UpdateProducts(newProducts);
        }
    }
}
