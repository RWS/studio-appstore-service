using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common.Interface
{
    public interface IProductsRepository
    {
        Task<IEnumerable<ProductDetails>> GetAllProducts();
        Task<IEnumerable<ParentProduct>> GetAllParentProducts();
        Task UpdateProducts(List<ProductDetails> products);
        Task UpdateParentProducts(List<ParentProduct> products);
        Task DeleteProduct(string id);
        Task DeleteParentProduct(string id);
    }
}
