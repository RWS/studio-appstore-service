using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IResponseManager _responseManager;
        private readonly IPluginRepository _pluginRepository;

        public ProductsRepository(IResponseManager responseManager, IPluginRepository pluginRepository)
        {
            _responseManager = responseManager;
            _pluginRepository = pluginRepository;
        }

        public async Task<bool> TryUpdateProduct(ProductDetails product)
        {
            var products = (await GetAllProducts()).ToList();
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

            await SaveProducts(products);
            return true;
        }

        public async Task<bool> TryUpdateProduct(ParentProduct parent)
        {
            var parents = (await GetAllParents()).ToList();
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

            await SaveParents(parents);
            return true;
        }

        public async Task DeleteProduct(string id)
        {
            var products = await GetAllProducts();
            await SaveProducts(products.Where(item => item.Id != id));
        }

        public async Task DeleteParent(string id)
        {
            var parents = await GetAllParents();
            await SaveParents(parents.Where(item => item.Id != id));
        }

        public async Task<IEnumerable<ProductDetails>> GetAllProducts()
        {
            var data = await _responseManager.GetResponse();
            return data.Products;
        }

        public async Task<IEnumerable<ParentProduct>> GetAllParents()
        {
            var data = await _responseManager.GetResponse();
            return data.ParentProducts;
        }

        public async Task<ParentProduct> GetParentById(string id)
        {
            var parents = await GetAllParents();
            return parents.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task<ProductDetails> GetProductById(string id)
        {
            var products = await GetAllProducts();
            return products.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task<bool> IsProductInUse(string id)
        {
            var plugins = await _pluginRepository.GetAll(null);
            return plugins.SelectMany(x => x.Versions
                          .SelectMany(y => y.SupportedProducts))
                          .Any(x => x == id);
        }

        private async Task SaveProducts(IEnumerable<ProductDetails> products)
        {
            var data = await _responseManager.GetResponse();
            data.Products = products;
            await _responseManager.SaveResponse(data);
        }

        private async Task SaveParents(IEnumerable<ParentProduct> products)
        {
            var data = await _responseManager.GetResponse();
            data.ParentProducts = products;
            await _responseManager.SaveResponse(data);
        }
    }
}
