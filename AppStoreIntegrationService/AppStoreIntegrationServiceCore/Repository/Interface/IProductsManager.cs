using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsManager
    {
        Task SaveProducts(IEnumerable<ProductDetails> products);
        Task SaveProducts(IEnumerable<ParentProduct> products);
        Task<IEnumerable<ProductDetails>> ReadProducts();
        Task<IEnumerable<ParentProduct>> ReadParents();
    }
}
