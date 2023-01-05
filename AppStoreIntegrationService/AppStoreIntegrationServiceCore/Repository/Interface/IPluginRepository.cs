using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task UpdatePlugin(PluginDetails<PluginVersion<string>, string> plugin);
        Task AddPlugin(PluginDetails<PluginVersion<string>, string> plugin);
        Task UpdatePluginVersion(int pluginId, PluginVersion<string> version);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginDetails<PluginVersion<string>, string>> GetPluginById(int id, ClaimsPrincipal user = null);
        IEnumerable<PluginDetails<PluginVersion<string>, string>> SearchPlugins(IEnumerable<PluginDetails<PluginVersion<string>, string>> pluginsList, PluginFilter filter, IEnumerable<ProductDetails> products);
        Task<IEnumerable<PluginDetails<PluginVersion<string>, string>>> GetAll(string sortOrder, ClaimsPrincipal user = null);
    }
}
