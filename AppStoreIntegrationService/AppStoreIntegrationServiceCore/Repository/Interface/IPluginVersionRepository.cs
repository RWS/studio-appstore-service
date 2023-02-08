using AppStoreIntegrationServiceCore.Model;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IPluginVersionRepository
    {
        Task Save(int pluginId, PluginVersion version, bool removeOtherVersions = false);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, Status status = Status.All);
        Task<IEnumerable<PluginVersion>> GetPluginVersions(int pluginId, Status status = Status.All);
        Task<bool> HasActiveChanges(int pluginId, string versionId);
        Task<bool> ExistsVersion(int pluginId, string versionId);
        Task<bool> HasPendingChanges(int pluginId, string versionId, string userRole = null);
        Task<bool> HasDraftChanges(int pluginId, string versionId, string userRole = null);
    }
}
