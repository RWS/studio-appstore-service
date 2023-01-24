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

        public async Task SavePlugin(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var plugins = plugin.Status switch
            {
                Status.Draft => new List<PluginDetails>(await _pluginManager.ReadDrafts()),
                Status.InReview => new List<PluginDetails>(await _pluginManager.ReadPending()),
                _ => new List<PluginDetails>(await _pluginManager.ReadPlugins()),
            };

            plugins = await UpdatePlugins(plugin, plugins);
            await SaveDrafts(plugins, plugin, removeOtherVersions);
            await SavePending(plugins, plugin, removeOtherVersions);
            await SavePlugins(plugins, plugin, removeOtherVersions);
        }

        private async Task SavePlugins(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status == Status.Active || plugin.Status == Status.Inactive)
            {
                await _pluginManager.SavePlugins(plugins);

                if (removeOtherVersions)
                {
                    var pending = await _pluginManager.ReadPending();
                    await _pluginManager.SavePending(pending.Where(p => p.Id != plugin.Id));
                }
            }
        }

        private async Task SavePending(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status == Status.InReview)
            {
                await _pluginManager.SavePending(plugins);
                if (removeOtherVersions)
                {
                    var drafts = await _pluginManager.ReadDrafts();
                    await _pluginManager.SaveDrafts(drafts.Where(p => p.Id != plugin.Id));
                }
            }
        }

        private async Task SaveDrafts(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status == Status.Draft)
            {
                await _pluginManager.SaveDrafts(plugins);
                if (removeOtherVersions)
                {
                    var pending = await _pluginManager.ReadPending();
                    await _pluginManager.SavePending(pending.Where(p => p.Id != plugin.Id));
                }
            }
        }

        private async Task<List<PluginDetails>> UpdatePlugins(PluginDetails plugin, List<PluginDetails> plugins)
        {
            var allPlugins = await GetAll(null);
            var old = plugins.FirstOrDefault(p => p.Id == plugin.Id);
            var index = plugins.IndexOf(old);

            if (index < 0)
            {
                if (allPlugins.Where(p => p.Name == plugin.Name && p.Id != plugin.Id).Count() > 0)
                {
                    throw new Exception($"Another plugin with the name {plugin.Name} already exists");
                }

                plugins.Add(plugin);
            }
            else
            {
                if (allPlugins.Where(p => p.Name == plugin.Name && p.Id != plugin.Id).Count() > 1)
                {
                    throw new Exception($"Another plugin with the name {plugin.Name} already exists");
                }

                plugin.Versions = plugin.Versions.Any() ? plugin.Versions : old.Versions;
                plugins[index] = plugin;
            }

            return plugins;
        }

        public async Task<PluginDetails> GetDraftById(int id, ClaimsPrincipal user)
        {
            var drafts = await _pluginManager.ReadDrafts();
            return drafts?.FirstOrDefault(p => p.Id == id);
        }

        public async Task<PluginDetails> GetPendingById(int id, ClaimsPrincipal user)
        {
            var plugins = await _pluginManager.ReadPending();
            return plugins?.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task<PluginDetails> GetPluginById(int id, Status status = Status.All, ClaimsPrincipal user = null)
        {
            var plugins = status switch
            {
                Status.Draft => await _pluginManager.ReadDrafts(),
                Status.InReview => await _pluginManager.ReadPending(),
                Status.Active => await _pluginManager.ReadPlugins(),
                _ => await GetAll(null, user)
            };

            return plugins?.FirstOrDefault(p => p.Id.Equals(id));
        }

        public async Task RemovePlugin(int id)
        {
            var drafts = await _pluginManager.ReadDrafts();
            var pending = await _pluginManager.ReadPending();
            var plugins = await _pluginManager.ReadPlugins();

            await _pluginManager.SavePlugins(plugins.Where(p => !p.Id.Equals(id)));
            await _pluginManager.SavePending(pending.Where(p => !p.Id.Equals(id)));
            await _pluginManager.SaveDrafts(drafts.Where(p => !p.Id.Equals(id)));
        }

        public async Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, ClaimsPrincipal user = null)
        {
            var drafts = await _pluginManager.ReadDrafts();
            var pending = await _pluginManager.ReadPending();
            var plugins = await _pluginManager.ReadPlugins();
            plugins = plugins.Concat(pending).Concat(drafts).DistinctBy(p => p.Id);

            plugins = sortOrder?.ToLower() switch
            {
                "asc" => plugins?.OrderByDescending(p => p.Name).ToList(),
                _ => plugins?.OrderBy(p => p.Name).ToList()
            };

            if (user?.IsInRole("Developer") ?? false)
            {
                return plugins?.Where(p => p.Developer.DeveloperName == user.Identity.Name);
            }

            if (user?.IsInRole("Administrator") ?? false)
            {
                return plugins.SkipWhile(p => p.Status.Equals(Status.Draft) && !p.HasAdminConsent);
            }

            if (user?.IsInRole("StandardUser") ?? false)
            {
                return plugins?.Where(p => p.Status.Equals(Status.Active) || p.Status.Equals(Status.Inactive));
            }

            return plugins;
        }

        public async Task<bool> ExitsPlugin(int id)
        {
            if (id < 0)
            {
                return false;
            }

            var drafts = await _pluginManager.ReadDrafts();
            var pending = await _pluginManager.ReadPending();
            var production = await _pluginManager.ReadPlugins();

            return drafts.Concat(pending).Concat(production).Any(p => p.Id == id);
        }

        public async Task<bool> HasActiveChanges(int id)
        {
            var plugins = await _pluginManager.ReadPlugins();
            return plugins.Any(p => p.Id == id);
        }

        public async Task<bool> HasPendingChanges(int id, ClaimsPrincipal user = null)
        {
            var pending = await _pluginManager.ReadPending();
            if ((user?.IsInRole("Administrator") ?? false) || (user?.IsInRole("Developer") ?? false))
            {
                return pending.Any(p => p.Id == id);
            }

            return false;
        }

        public async Task<bool> HasDraftChanges(int id, ClaimsPrincipal user = null)
        {
            var drafts = await _pluginManager.ReadDrafts();
            if (user?.IsInRole("Administrator") ?? false)
            {
                return drafts.Any(p => p.Id == id && p.HasAdminConsent);
            }

            if (user?.IsInRole("Developer") ?? false)
            {
                return drafts.Any(p => p.Id == id);
            }

            return false;
        }

        public async Task<PluginDetails> GetActiveById(int id, ClaimsPrincipal user = null)
        {
            var plugins = await _pluginManager.ReadPlugins();
            return plugins.FirstOrDefault(p => p.Id == id);
        }
    }
}
