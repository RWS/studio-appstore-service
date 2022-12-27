using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IProductsManager _productsManager;
        private readonly List<ProductDetails> _defaultProducts;
        private readonly List<ParentProduct> _defaultParentProducts;

        public ProductsRepository(IProductsManager productsManager)
        {
            _productsManager = productsManager;
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
            await _productsManager.SaveProducts(products);
        }

        public async Task UpdateProducts(List<ParentProduct> products)
        {
            await _productsManager.SaveProducts(products);
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
            return (Products: await _productsManager.ReadProducts() ?? _defaultProducts, Parents: await _productsManager.ReadParents() ?? _defaultParentProducts);
        }
    }
}
