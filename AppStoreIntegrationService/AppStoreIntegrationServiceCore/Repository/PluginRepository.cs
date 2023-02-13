using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class PluginRepository : IPluginRepository
    {
        private readonly IResponseManager _responseManager;

        public PluginRepository(IResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task SavePlugin(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var plugins = plugin.Status switch
            {
                Status.Draft => new List<PluginDetails>(await GetAll(null, status: Status.Draft)),
                Status.InReview => new List<PluginDetails>(await GetAll(null, status: Status.InReview)),
                _ => new List<PluginDetails>(await GetAll(null, status: Status.Active)),
            };

            plugins = await UpdatePlugins(plugin, plugins);
            await SaveDrafts(plugins, plugin, removeOtherVersions);
            await SavePending(plugins, plugin, removeOtherVersions);
            await SaveActive(plugins, plugin, removeOtherVersions);
        }

        public async Task<PluginDetails> GetPluginById(int id, string username = null, string userRole = null, Status status = Status.All)
        {
            var plugins = await GetAll(null, username, userRole, status);
            return plugins?.FirstOrDefault(p => p.Id == id);
        }

        public async Task RemovePlugin(int id)
        {
            var data = await _responseManager.GetResponse();
            var drafts = data.Drafts;
            var pending = data.Pending;
            var plugins = data.Value;

            await SavePlugins(plugins.Where(p => p.Id != id), Status.Active);
            await SavePlugins(pending.Where(p => p.Id != id), Status.InReview);
            await SavePlugins(drafts.Where(p => p.Id != id), Status.Draft);
        }

        public async Task<IEnumerable<PluginDetails>> GetAll(string sortOrder, string username = null, string userRole = null, Status status = Status.All)
        {
            var data = await _responseManager.GetResponse();
            var drafts = data.Drafts;
            var pending = data.Pending;
            var plugins = data.Value;

            plugins = status switch
            {
                Status.Draft => drafts,
                Status.InReview => pending,
                Status.Active => plugins,
                Status.Inactive => plugins,
                _ => plugins.Concat(pending).Concat(drafts).DistinctBy(p => p.Id)
            };

            plugins = sortOrder?.Equals("asc", StringComparison.CurrentCultureIgnoreCase) ?? false ? plugins?.OrderBy(p => p.Name) : plugins?.OrderByDescending(p => p.Name);

            return userRole switch
            {
                "Developer" => plugins?.Where(p => p.Developer.DeveloperName == username),
                "Administrator" => plugins?.Where(p => p.Status != Status.Draft || p.HasAdminConsent),
                "StandardUser" => plugins?.Where(p => p.Status == Status.Active || p.Status == Status.Inactive),
                _ => plugins
            };
        }

        public async Task<bool> ExitsPlugin(int id)
        {
            if (id < 0)
            {
                return false;
            }

            var plugins = await GetAll(null);
            return plugins.Any(p => p.Id == id);
        }

        public async Task<bool> HasActiveChanges(int id)
        {
            var plugins = await GetAll(null, status: Status.Active);
            return plugins.Any(p => p.Id == id);
        }

        public async Task<bool> HasPendingChanges(int id, string userRole = null)
        {
            var plugins = await GetAll(null, status: Status.InReview);
            return userRole != "StandardUser" && plugins.Any(p => p.Id == id);
        }

        public async Task<bool> HasDraftChanges(int id, string userRole = null)
        {
            var drafts = await GetAll(null, status: Status.Draft);
            return userRole switch
            {
                "Administrator" => drafts.Any(p => p.Id == id && p.HasAdminConsent),
                "Developer" => drafts.Any(p => p.Id == id),
                _ => false
            };
        }

        private async Task SaveActive(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status != Status.Active && plugin.Status != Status.Inactive)
            {
                return;
            }

            await SavePlugins(plugins, Status.Active);
            if (removeOtherVersions)
            {
                var pending = await GetAll(null, status: Status.InReview);
                await SavePlugins(pending.Where(p => p.Id != plugin.Id), Status.InReview);
            }
        }

        private async Task SavePending(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status != Status.InReview)
            {
                return;
            }

            await SavePlugins(plugins, Status.InReview);
            if (removeOtherVersions)
            {
                var drafts = await GetAll(null, status: Status.Draft);
                await SavePlugins(drafts.Where(p => p.Id != plugin.Id), Status.Draft);
            }
        }

        private async Task SaveDrafts(List<PluginDetails> plugins, PluginDetails plugin, bool removeOtherVersions)
        {
            if (plugin.Status != Status.Draft)
            {
                return;
            }

            await SavePlugins(plugins, Status.Draft);
            if (removeOtherVersions)
            {
                var pending = await GetAll(null, status: Status.InReview);
                await SavePlugins(pending.Where(p => p.Id != plugin.Id), Status.InReview);
            }
        }

        private async Task SavePlugins(IEnumerable<PluginDetails> plugins, Status status)
        {
            var data = await _responseManager.GetResponse();
            switch (status)
            {
                case Status.Draft:
                    data.Drafts = plugins;
                    break;
                case Status.InReview:
                    data.Pending = plugins;
                    break;
                case Status.Active:
                    data.Value = plugins;
                    break;
                default: break;
            }

            await _responseManager.SaveResponse(data);
        }

        private async Task<List<PluginDetails>> UpdatePlugins(PluginDetails plugin, List<PluginDetails> plugins)
        {
            var allPlugins = await GetAll("asc");
            var old = plugins.FirstOrDefault(p => p.Id == plugin.Id);
            var index = plugins.IndexOf(old);

            if (index < 0)
            {
                if (allPlugins.Any(p => p.Name == plugin.Name && p.Id != plugin.Id))
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

                plugins[index] = plugin;
            }

            return plugins;
        }
    }
}
