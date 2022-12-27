using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsManager
    {
        Task SaveProducts(List<ProductDetails> products);
        Task SaveProducts(List<ParentProduct> products);
        Task<List<ProductDetails>> ReadProducts();
        Task<List<ParentProduct>> ReadParents();
    }
}
