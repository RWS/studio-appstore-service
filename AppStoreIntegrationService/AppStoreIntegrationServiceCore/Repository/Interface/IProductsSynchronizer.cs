using AppStoreIntegrationServiceCore.Model;
using static AppStoreIntegrationServiceCore.Enums;

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
