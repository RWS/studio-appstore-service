using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Security.Claims;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class PluginVersionRepository : IPluginVersionRepository
    {
        private readonly IPluginManager _pluginManager;
        private readonly IPluginRepository _pluginRepository;

        public PluginVersionRepository(IPluginManager pluginManager, IPluginRepository pluginRepository)
        {
            _pluginManager = pluginManager;
            _pluginRepository = pluginRepository;
        }

        public async Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId, Status.All, user);
            return plugin.Versions.FirstOrDefault(v => v.VersionId.Equals(versionId));
        }

        public async Task RemovePluginVersion(int id, string versionId)
        {
            var plugins = await _pluginManager.ReadPlugins();
            var plugin = plugins.FirstOrDefault(p => p.Id == id);
            var version = plugin.Versions.FirstOrDefault(v => v.VersionId == versionId);
            plugin.Versions.Remove(version);
            await _pluginManager.SavePlugins(plugins);
        }

        public async Task UpdatePluginVersion(int id, PluginVersion version)
        {
            var plugins = await _pluginManager.ReadPlugins();
            var plugin = plugins.FirstOrDefault(p => p.Id == id);
            var old = plugin.Versions?.FirstOrDefault(v => v.VersionId == version.VersionId);

            if (old == null)
            {
                plugin.Versions.Add(version);
            }
            else
            {
                var index = plugin.Versions.IndexOf(old);
                plugin.Versions[index] = version;
            }

            plugin.DownloadUrl = version.DownloadUrl;
            await _pluginRepository.SavePlugin(plugin);
        }
    }
}
