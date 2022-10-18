using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using Microsoft.AspNetCore.Http;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface IPluginRepositoryExtended<T> : IPluginRepository<T>
    {
        Task RemovePlugin(int id);
        Task RemovePluginVersion(int pluginId, string versionId);
        Task<bool> TryImportPluginsFromFile(IFormFile file);
        Task<T> GetPluginById(int id);
        Task UpdatePrivatePlugin(PrivatePlugin<PluginVersion<string>> plugin);
        Task AddPrivatePlugin(PrivatePlugin<PluginVersion<string>> plugin);
        Task SaveToFile(List<T> pluginsList);
    }
}
