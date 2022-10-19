using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;

namespace AppStoreIntegrationServiceCore.Repository.Interface
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
