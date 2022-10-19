using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public enum ProductType
    {
        Child,
        Parent
    }

    public class ProductsRepository<T> : IProductsRepository where T : PluginDetails<PluginVersion<string>>, new()
    {
        protected readonly IAzureRepositoryExtended<T> _azureRepositoryExtended;
        protected readonly IConfigurationSettings _configurationSettings;
        private readonly ILocalRepositoryExtended<T> _localRepositoryExtended;
        protected List<ProductDetails> _defaultProducts;
        protected List<ParentProduct> _defaultParentProducts;

        public ProductsRepository(IAzureRepositoryExtended<T> azureRepositoryExtended, IConfigurationSettings configurationSettings, ILocalRepositoryExtended<T> localRepositoryExtended)
        {
            _azureRepositoryExtended = azureRepositoryExtended;
            _configurationSettings = configurationSettings;
            _localRepositoryExtended = localRepositoryExtended;
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
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepositoryExtended.UpdateProductsFileBlob(products);
                return;
            }

            await _localRepositoryExtended.SaveProductsToFile(products);
        }

        public async Task UpdateProducts(List<ParentProduct> products)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepositoryExtended.UpdateParentsFileBlob(products);
                return;
            }

            await _localRepositoryExtended.SaveParentsToFile(products);
        }

        public async Task DeleteProduct(string id, ProductType type)
        {
            await InitProductLists();
            if (type == ProductType.Child)
            {
                await UpdateProducts(_defaultProducts.Where(item => item.Id != id).ToList());
                return;
            }

            await UpdateProducts(_defaultParentProducts.Where(item => item.ParentId != id).ToList());

        }

        public async Task<List<ProductDetails>> GetAllProducts()
        {
            await InitProductLists();
            return _defaultProducts;
        }

        public async Task<List<ParentProduct>> GetAllParents()
        {
            await InitProductLists();
            return _defaultParentProducts;
        }

        private async Task InitProductLists()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                _defaultProducts = await _localRepositoryExtended.ReadProductsFromFile();
                _defaultParentProducts = await _localRepositoryExtended.ReadParentsFromFile();
                return;
            }

            _defaultProducts = await _azureRepositoryExtended.GetProductsFromContainer() ?? _defaultProducts;
            _defaultParentProducts = await _azureRepositoryExtended.GetParentProductsFromContainer() ?? _defaultParentProducts ;
        }
    }
}
