using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsSynchronizer
    {
        Task SyncOnUpdate(List<SupportedProductDetails> products);

        Task Sync();

        Task<bool> IsInUse(string id);

        Task SyncOnImport(List<PluginDetails> plugins);
    }
}
