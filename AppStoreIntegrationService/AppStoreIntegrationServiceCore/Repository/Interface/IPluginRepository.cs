using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginDetails<PluginVersion<string>, string>> GetPluginById(int id, string developerName = null);
        Task UpdatePlugin(PluginDetails<PluginVersion<string>, string> plugin);
        Task AddPlugin(PluginDetails<PluginVersion<string>, string> plugin);
        List<PluginDetails<PluginVersion<string>, string>> SearchPlugins(List<PluginDetails<PluginVersion<string>, string>> pluginsList, PluginFilter filter, List<ProductDetails> products);
        Task<List<PluginDetails<PluginVersion<string>, string>>> GetAll(string sortOrder, string developerName = null);
    }
}
