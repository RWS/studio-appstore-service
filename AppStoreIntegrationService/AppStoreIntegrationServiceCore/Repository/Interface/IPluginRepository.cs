using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository<T>
    {
        Task RemovePlugin(int id);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<T> GetPluginById(int id, string developerName = null);
        Task UpdatePrivatePlugin(PrivatePlugin<PluginVersion<string>> plugin);
        Task AddPrivatePlugin(PrivatePlugin<PluginVersion<string>> plugin);
        Task SaveToFile(List<T> pluginsList);
        List<T> SearchPlugins(List<T> pluginsList, PluginFilter filter, List<ProductDetails> products);
        Task<List<T>> GetAll(string sortOrder, string developerName = null);
    }
}
