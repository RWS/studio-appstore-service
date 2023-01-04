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
                    Id = "1",
                    ProductName = "Trados Studio"
                }
            };
        }

        public async Task<bool> TryUpdateProduct(ProductDetails product)
        {
            var products = await _productsManager.ReadProducts();
            if (products.Any(p => p.ProductName == product.ProductName && p.Id != product.Id))
            {
                return false;
            }

            var index = products.IndexOf(products.FirstOrDefault(p => p.Id == product.Id));
            if (index >= 0)
            {
                products[index] = product;
            }
            else
            {
                products.Add(product);
            }

            await _productsManager.SaveProducts(products);
            return true;
        }

        public async Task<bool> TryUpdateProduct(ParentProduct parent)
        {
            var parents = await _productsManager.ReadParents();
            if (parents.Any(p => p.ProductName == parent.ProductName && p.Id != parent.Id))
            {
                return false;
            }

            var index = parents.IndexOf(parents.FirstOrDefault(p => p.Id == parent.Id));
            if (index >= 0)
            {
                parents[index] = parent;
            }
            else
            {
                parents.Add(parent);
            }

            await _productsManager.SaveProducts(parents);
            return true;
        }

        public async Task DeleteProduct(string id, ProductType type)
        {
            var (Products, Parents) = await GetProductsFromPossibleLocations();
            if (type == ProductType.Child)
            {
                await _productsManager.SaveProducts(Products.Where(item => item.Id != id).ToList());
                return;
            }

            await _productsManager.SaveProducts(Parents.Where(item => item.Id != id).ToList());
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

        public async Task<ParentProduct> GetParentById(string id)
        {
            var parents = await _productsManager.ReadParents();
            return parents.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task<ProductDetails> GetProductById(string id)
        {
            var products = await _productsManager.ReadProducts();
            return products.FirstOrDefault(p => p.Id.Equals(id));
        }
    }
}
