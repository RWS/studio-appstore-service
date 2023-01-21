using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Security.Claims;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class PluginRepository : IPluginRepository
    {
        private readonly IPluginManager _pluginManager;

        public PluginRepository(IPluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public async Task SavePlugin(PluginDetails plugin)
        {
            var plugins = new List<PluginDetails>(await _pluginManager.ReadPlugins());

            if (plugins.Where(p => p.Name == plugin.Name).Count() > 1)
            {
                throw new Exception($"Another plugin with the name {plugin.Name} already exists");
            }

            await _pluginManager.BackupPlugins(plugins);
            var old = plugins.FirstOrDefault(p => p.Id == plugin.Id);
            var index = plugins.IndexOf(old);

            if (index < 0)
            {
                plugin.Id = plugins.MaxBy(p => p.Id)?.Id + 1 ?? 0;
                await _pluginManager.SavePlugins(plugins.Append(plugin));
                return;
            }

            plugin.Versions = plugin.Versions.Any() ? plugin.Versions : old.Versions;
            plugins[index] = plugin;
            await _pluginManager.SavePlugins(plugins);
        }

        public async Task<PluginDetails> GetPluginById(int id, ClaimsPrincipal user = null)
        {
            var plugins = await GetAll("asc", user);
            return plugins?.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task<PluginVersion> GetPluginVersion(int pluginId, string versionId, ClaimsPrincipal user = null)
        {
            var plugin = await GetPluginById(pluginId, user);
            return plugin.Versions.FirstOrDefault(v => v.VersionId.Equals(versionId));
        }

        public async Task RemovePluginVersion(int id, string versionId)
        {
            var plugins = await _pluginManager.ReadPlugins();
            var plugin = plugins.FirstOrDefault(p => p.Id == id);
            var version = plugin.Versions.FirstOrDefault(v => v.VersionId == versionId);
            await _pluginManager.BackupPlugins(plugins);
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
            await SavePlugin(plugin);
        }

        public async Task RemovePlugin(int id)
        {
            var plugins = await _pluginManager.ReadPlugins();
            await _pluginManager.BackupPlugins(plugins);
            await _pluginManager.SavePlugins(plugins.Where(p => !p.Id.Equals(id)));
        }

        public async Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, ClaimsPrincipal user = null)
        {
            var condition = !string.IsNullOrEmpty(sortOrder) && !sortOrder.Equals("asc", StringComparison.CurrentCultureIgnoreCase);
            var plugins = await _pluginManager.ReadPlugins();

            plugins = condition switch
            {
                true => plugins?.OrderByDescending(p => p.Name).ToList(),
                _ => plugins?.OrderBy(p => p.Name).ToList()
            };

            if (user?.IsInRole("Developer") ?? false)
            {
                return plugins?.Where(p => p.Developer.DeveloperName.Equals(user.Identity.Name));
            }

            if (user?.IsInRole("Administrator") ?? false)
            {
                return plugins.SkipWhile(p => p.Status.Equals(Status.Draft) && !p.HasAdminConsent);
            }

            return plugins?.Where(p => p.Status.Equals(Status.Active) || p.Status.Equals(Status.Inactive));
        }
    }
}
