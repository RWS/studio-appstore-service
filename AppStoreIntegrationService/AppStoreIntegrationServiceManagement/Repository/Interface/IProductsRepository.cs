using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface IProductsRepository
    {
        Task<IEnumerable<ProductDetails>> GetAllProducts();
        Task<IEnumerable<ParentProduct>> GetAllParents();
        Task<ParentProduct> GetParentById(string id);
        Task<ProductDetails> GetProductById(string id);
        Task<bool> TryUpdateProduct(ProductDetails products);
        Task<bool> TryUpdateProduct(ParentProduct products);
        Task DeleteProduct(string id);
        Task DeleteParent(string id);
        Task<bool> IsProductInUse(string id);
    }
}
