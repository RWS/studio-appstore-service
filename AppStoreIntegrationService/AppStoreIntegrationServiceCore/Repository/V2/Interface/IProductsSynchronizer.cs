using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface IProductsSynchronizer
    {
        Task<bool> IsInUse(string id, ProductType type);
        bool ExistDuplicate(IEnumerable<ParentProduct> products);
        bool ExistDuplicate(IEnumerable<ProductDetails> products);
        string SetIndex(IEnumerable<ProductDetails> products);
        string SetIndex(IEnumerable<ParentProduct> products);
    }
}
