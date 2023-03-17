using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Repository.Interface;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class PluginVersionRepository : IPluginVersionRepository
    {
        private readonly IPluginRepository _pluginRepository;

        public PluginVersionRepository(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        public async Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, Status status = Status.All)
        {
            var versions = await GetPluginVersions(pluginId, status: status);
            return versions.FirstOrDefault(v => v.VersionId == versionId);
        }

        public async Task<IEnumerable<PluginVersion>> GetPluginVersions(int pluginId, Status status = Status.All)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);

            if (plugin == null)
            {
                return Enumerable.Empty<PluginVersion>();
            }

            return status switch
            {
                Status.Draft => plugin.Drafts,
                Status.InReview => plugin.Pending,
                Status.Active => plugin.Versions,
                Status.Inactive => plugin.Versions,
                _ => plugin.Versions.Concat(plugin.Pending).Concat(plugin.Drafts).DistinctBy(v => v.VersionId)
            };
        }

        public async Task<bool> HasActiveChanges(int pluginId, string versionId)
        {
            var plugins = await _pluginRepository.GetAll("asc");
            var plugin = plugins.FirstOrDefault(p => p.Id == pluginId);

            if (plugin == null)
            {
                return false;
            }

            return plugin.Versions.Any(v => v.VersionId == versionId);
        }

        public async Task<bool> HasDraftChanges(int pluginId, string versionId, string userRole)
        {
            var plugins = await _pluginRepository.GetAll("asc");
            var plugin = plugins.FirstOrDefault(p => p.Id == pluginId);

            if (plugin == null)
            {
                return false;
            }

            return userRole switch
            {
                "Administrator" => plugin.Drafts.Any(v => v.VersionId == versionId && v.HasAdminConsent),
                "Developer" => plugin.Drafts.Any(v => v.VersionId == versionId),
                _ => false
            };
        }

        public async Task<bool> HasPendingChanges(int pluginId, string versionId, string userRole)
        {
            var plugins = await _pluginRepository.GetAll("asc");
            var plugin = plugins.FirstOrDefault(p => p.Id == pluginId);

            if (plugin == null)
            {
                return false;
            }

            return plugin.Pending.Any(v => v.VersionId == versionId);
        }

        public async Task RemovePluginVersion(int pluginId, string versionId)
        {
            var plugins = await _pluginRepository.GetAll("asc");
            var plugin = plugins.FirstOrDefault(p => p.Id == pluginId);

            if (plugin == null)
            {
                return;
            }

            plugin.Versions = plugin.Versions.Where(v => v.VersionId != versionId).ToList();
            plugin.Pending = plugin.Pending.Where(v => v.VersionId != versionId).ToList();
            plugin.Drafts = plugin.Drafts.Where(v => v.VersionId != versionId).ToList();

            await _pluginRepository.SavePlugin(plugin);
        }

        public async Task Save(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugins = await _pluginRepository.GetAll("asc");
            var plugin = plugins.FirstOrDefault(p => p.Id == pluginId);
            var versions = version.VersionStatus switch
            {
                Status.Draft => plugin.Drafts,
                Status.InReview => plugin.Pending,
                _ => plugin.Versions
            };

            versions = UpdateVersions(version, versions);
            await SaveDrafts(plugin, version, versions, removeOtherVersions);
            await SavePending(plugin, version, versions, removeOtherVersions);
            await SaveActiveVersion(plugin, version, versions, removeOtherVersions);
        }

        private async Task SaveActiveVersion(PluginDetails plugin, PluginVersion version, List<PluginVersion> versions, bool removeOtherVersions)
        {
            if (version.VersionStatus != Status.Active && version.VersionStatus != Status.Inactive)
            {
                return;
            }

            plugin.Versions = versions;

            if (removeOtherVersions)
            {
                plugin.Pending = plugin.Pending.Where(p => p.VersionId != version.VersionId).ToList();
            }

            await _pluginRepository.SavePlugin(plugin);
        }

        private async Task SavePending(PluginDetails plugin, PluginVersion version, List<PluginVersion> versions, bool removeOtherVersions)
        {
            if (version.VersionStatus != Status.InReview)
            {
                return;
            }

            plugin.Pending = versions;

            if (removeOtherVersions)
            {
                plugin.Drafts = plugin.Drafts.Where(v => v.VersionId != version.VersionId).ToList();
            }

            await _pluginRepository.SavePlugin(plugin);
        }

        private async Task SaveDrafts(PluginDetails plugin, PluginVersion version, List<PluginVersion> versions, bool removeOtherVersions)
        {
            if (version.VersionStatus != Status.Draft)
            {
                return;
            }

            plugin.Drafts = versions;

            if (removeOtherVersions)
            {
                plugin.Pending = plugin.Pending.Where(v => v.VersionId != version.VersionId).ToList();
            }

            await _pluginRepository.SavePlugin(plugin);
        }

        private static List<PluginVersion> UpdateVersions(PluginVersion version, List<PluginVersion> versions)
        {
            var old = versions.FirstOrDefault(v => v.VersionId == version.VersionId);

            if (old == null)
            {
                version.CreatedAt = DateTime.Now;
                versions.Add(version);
            }
            else
            {
                var index = versions.IndexOf(old);
                versions[index] = version;
            }

            return versions;
        }

        public async Task<bool> ExistsVersion(int pluginId, string versionId)
        {
            if (pluginId < 0 || string.IsNullOrEmpty(versionId))
            {
                return false;
            }

            var versions = await GetPluginVersions(pluginId);
            return versions.Any(v => v.VersionId == versionId);
        }
    }
}
