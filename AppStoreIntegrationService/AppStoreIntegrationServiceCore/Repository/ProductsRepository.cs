using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public enum ProductType
    {
        Child,
        Parent
    }

    public class ProductsRepository<T> : IProductsRepository where T : PluginDetails<PluginVersion<string>, string>, new()
    {
        protected readonly IAzureRepository<T> _azureRepository;
        protected readonly IConfigurationSettings _configurationSettings;
        private readonly ILocalRepository<T> _localRepository;
        protected List<ProductDetails> _defaultProducts;
        protected List<ParentProduct> _defaultParentProducts;

        public ProductsRepository(IAzureRepository<T> azureRepository, IConfigurationSettings configurationSettings, ILocalRepository<T> localRepository)
        {
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
            _localRepository = localRepository;
            _defaultProducts = new List<ProductDetails>
            {
                new ProductDetails
                {
                    Id = "1",
                    ProductName = "SDL Trados Studio 2021",
                    ParentProductID = "1",
                    MinimumStudioVersion = "16.0"
                },
                new ProductDetails
                {
                    Id = "2",
                    ProductName = "Trados Studio 2022",
                    ParentProductID = "1",
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
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdateProductsFileBlob(products);
                return;
            }

            await _localRepository.SaveProductsToFile(products);
        }

        public async Task UpdateProducts(List<ParentProduct> products)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdateParentsFileBlob(products);
                return;
            }

            await _localRepository.SaveParentsToFile(products);
        }

        public async Task DeleteProduct(string id, ProductType type)
        {
            var (Products, Parents) = await GetProductsFromPossibleLocations();
            if (type == ProductType.Child)
            {
                await UpdateProducts(Products.Where(item => item.Id != id).ToList());
                return;
            }

            await UpdateProducts(Parents.Where(item => item.ParentId != id).ToList());

        }

        public async Task<List<ProductDetails>> GetAllProducts()
        {
            var (Products, _) = await GetProductsFromPossibleLocations();
            return Products;
        }

        public async Task<List<ParentProduct>> GetAllParents()
        {
            var (_, Parents) = await GetProductsFromPossibleLocations();
            return Parents;
        }

        private async Task<(List<ProductDetails> Products, List<ParentProduct> Parents)> GetProductsFromPossibleLocations()
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return (Products: await _localRepository.ReadProductsFromFile() ?? _defaultProducts,
                        Parents: await _localRepository.ReadParentsFromFile() ?? _defaultParentProducts);
            }

            return (Products: await _azureRepository.GetProductsFromContainer() ?? _defaultProducts,
                    Parents: await _azureRepository.GetParentProductsFromContainer() ?? _defaultParentProducts);
        }
    }
}
