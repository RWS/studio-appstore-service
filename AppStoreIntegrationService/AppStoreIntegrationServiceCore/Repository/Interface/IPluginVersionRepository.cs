using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginVersionRepository
    {
        Task Save(int pluginId, PluginVersion version, bool removeOtherVersions = false);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null);
        Task<bool> HasActiveChanges(int pluginId, string versionId);
        Task<bool> HasPendingChanges(int pluginId, string versionId, ClaimsPrincipal user = null);
        Task<bool> HasDraftChanges(int pluginId, string versionId, ClaimsPrincipal user = null);
    }
}
