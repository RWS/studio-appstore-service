using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsSynchronizer
    {
        Task SyncOnUpdate(List<ProductDetails> products);
        Task<bool> IsInUse(string id, ProductType type);
    }
}
