using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common.Interface
{
    public interface IProductsRepository
    {
        Task<List<ProductDetails>> GetAllProducts();
        Task<List<ParentProduct>> GetAllParents();
        Task UpdateProducts(List<ProductDetails> products);
        Task UpdateProducts(List<ParentProduct> products);
        Task DeleteProduct(string id, ProductType type);
    }
}
