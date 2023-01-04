using AppStoreIntegrationServiceCore.Model;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsRepository
    {
        Task<List<ProductDetails>> GetAllProducts();
        Task<List<ParentProduct>> GetAllParents();
        Task<ParentProduct> GetParentById(string id);
        Task<ProductDetails> GetProductById(string id);
        Task<bool> TryUpdateProduct(ProductDetails products);
        Task<bool> TryUpdateProduct(ParentProduct products);
        Task DeleteProduct(string id, ProductType type);
    }
}
