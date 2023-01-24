using AppStoreIntegrationServiceCore.Model;
using System.Security.Claims;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginVersionRepository
    {
        Task UpdatePluginVersion(int pluginId, PluginVersion version);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null);
    }
}
