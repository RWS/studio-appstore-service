using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task SavePlugin(PluginDetails plugin);
        Task UpdatePluginVersion(int pluginId, PluginVersion version);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null);
        Task<PluginDetails> GetPluginById(int id, ClaimsPrincipal user = null);
        Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, ClaimsPrincipal user = null);
    }
}
