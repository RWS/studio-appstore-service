using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginRepository
    {
        Task RemovePlugin(int id);
        Task SavePlugin(PluginDetails plugin, bool removeOtherVersions = false);
        Task UpdatePluginVersion(int pluginId, PluginVersion version);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null);
        Task<PluginDetails> GetPluginById(int id, ClaimsPrincipal user = null);
        Task<PluginDetails> GetDraftById(int id, ClaimsPrincipal user = null);
        Task<PluginDetails> GetActiveById(int id, ClaimsPrincipal user = null);
        Task<PluginDetails> GetPendingById(int id, ClaimsPrincipal user = null);
        Task<bool> ExitsPlugin(int id);
        Task<bool> HasActiveChanges(int id);
        Task<bool> HasPendingChanges(int id, ClaimsPrincipal user);
        Task<bool> HasDraft(int id, ClaimsPrincipal user = null);
        Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, ClaimsPrincipal user = null);
    }
}
