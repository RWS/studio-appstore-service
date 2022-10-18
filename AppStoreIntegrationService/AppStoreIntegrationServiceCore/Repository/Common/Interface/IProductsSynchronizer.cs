using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsSynchronizer
    {
        Task SyncOnUpdate(List<ProductDetails> products);

        Task<bool> IsInUse(string id);
    }
}
