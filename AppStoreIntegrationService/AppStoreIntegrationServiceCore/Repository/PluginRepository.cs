using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

        public async Task UpdatePlugin(PluginDetails plugin)
        {
            var plugins = (await _pluginManager.ReadPlugins()).ToList();

            if (plugins == null)
            {
                return;
            }

            if (plugins.Where(p => p.Name.Equals(plugin.Name)).Count() > 1)
            {
                throw new Exception($"Another plugin with the name {plugin.Name} already exists");
            }

            await _pluginManager.BackupPlugins(plugins);
            var old = plugins.FirstOrDefault(p => p.Id.Equals(plugin.Id));
            var index = plugins.IndexOf(old);

            if (!plugin.Versions.Any())
            {
                plugin.Versions = old.Versions;
            }

            plugins[index] = plugin;
            await _pluginManager.SavePlugins(plugins);
        }

        public async Task AddPlugin(PluginDetails plugin)
        {
            var plugins = await _pluginManager.ReadPlugins() ?? new List<PluginDetails>();

            if (!plugins.Any(p => p.Name.Equals(plugin.Name)))
            {
                await _pluginManager.BackupPlugins(plugins);
                plugin.Id = plugins?.MaxBy(p => p.Id)?.Id + 1 ?? 0;
                await _pluginManager.SavePlugins(plugins.Append(plugin));
            }
            else
            {
                throw new Exception($"Another plugin with the name {plugin.Name} already exists");
            }
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
            var plugin = plugins.FirstOrDefault(p => p.Id.Equals(id));
            var version = plugin.Versions.FirstOrDefault(v => v.VersionId.Equals(versionId));
            await _pluginManager.BackupPlugins(plugins);
            plugin.Versions.Remove(version);
            await _pluginManager.SavePlugins(plugins);
        }

        public async Task UpdatePluginVersion(int id, PluginVersion version)
        {
            var plugins = await _pluginManager.ReadPlugins();
            var plugin = plugins.FirstOrDefault(p => p.Id.Equals(id));
            var old = plugin.Versions?.FirstOrDefault(v => v.VersionId.Equals(version.VersionId));

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
            await UpdatePlugin(plugin);
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

        private static IEnumerable<PluginDetails> FilterByStatus(IEnumerable<PluginDetails> plugins, Status status)
        {
            if (status.Equals(Status.Active))
            {
                plugins = plugins.Where(x => x.Status.Equals(Status.Active));
                foreach (var plugin in plugins)
                {
                    plugin.Versions = plugin.Versions.Where(v => v.VersionStatus.Equals(Status.Active)).ToList();
                }
            }

            return status switch
            {
                Status.Inactive => plugins.Where(x => x.Status.Equals(Status.Inactive)),
                Status.Draft => plugins.Where(x => x.Status.Equals(Status.Draft) || x.Versions.Any(v => v.VersionStatus.Equals(Status.Draft))),
                Status.InReview => plugins.Where(x => x.Status.Equals(Status.InReview) || x.Versions.Any(v => v.VersionStatus.Equals(Status.InReview))),
                _ => plugins
            };
        }

        private static IEnumerable<PluginDetails> FilterByQuery(IEnumerable<PluginDetails> pluginsList, string query)
        {
            return pluginsList.Where(p => Regex.IsMatch(p.Name, query, RegexOptions.IgnoreCase));
        }

        private static IEnumerable<PluginDetails> FilterByPrice(IEnumerable<PluginDetails> pluginsList, string price)
        {
            return pluginsList.Where(p => p.PaidFor == (!string.IsNullOrEmpty(price) && price.Equals("paid", StringComparison.CurrentCultureIgnoreCase)));
        }

        private static IEnumerable<PluginDetails> FilterByVersion(IEnumerable<PluginDetails> pluginsList, string studioVersion, IEnumerable<ProductDetails> products)
        {
            var plugins = new List<PluginDetails>();
            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion>();

                foreach (var version in plugin.Versions)
                {
                    var product = version.SupportedProducts.SelectMany(sp => products
                                                           .Where(p => p.Id == sp))
                                                           .FirstOrDefault(s => s.ProductName.Equals(oldTradosName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName + "(SR2)"));
                    if (product != null)
                    {
                        matchingVersions.Add(version);
                        plugin.DownloadUrl = version.DownloadUrl;
                    }
                }

                if (matchingVersions.Any())
                {
                    plugin.Versions = matchingVersions;
                    plugins.Add(plugin);
                }
            }

            return plugins;
        }

        private static IEnumerable<PluginDetails> FilterByCategory(IEnumerable<PluginDetails> pluginsList, IEnumerable<int> categoryIds)
        {
            return categoryIds.SelectMany(c => pluginsList.Where(p => p.Categories.Any(pc => pc.Equals(c))));
        }

        public IEnumerable<PluginDetails> SearchPlugins(IEnumerable<PluginDetails> pluginsList, PluginFilter filter, IEnumerable<ProductDetails> products)
        {
            pluginsList ??= new List<PluginDetails>();

            var searchedPluginList = FilterByStatus(pluginsList, filter.Status);
            if (!string.IsNullOrEmpty(filter?.SupportedProduct))
            {
                searchedPluginList = FilterByProduct(searchedPluginList, filter.SupportedProduct);
            }

            if (!string.IsNullOrEmpty(filter?.Query))
            {
                searchedPluginList = FilterByQuery(searchedPluginList, filter.Query);
            }

            if (!string.IsNullOrEmpty(filter?.Price))
            {
                searchedPluginList = FilterByPrice(searchedPluginList, filter.Price);
            }

            if (!string.IsNullOrEmpty(filter?.StudioVersion))
            {
                searchedPluginList = FilterByVersion(searchedPluginList, filter.StudioVersion, products);
            }

            if (filter?.CategoryId?.Count > 0)
            {
                searchedPluginList = FilterByCategory(searchedPluginList, filter.CategoryId);
            }

            return searchedPluginList;
        }

        private static IEnumerable<PluginDetails> FilterByProduct(IEnumerable<PluginDetails> plugins, string product)
        {
            return plugins.Where(p => p.Versions.Select(v => v.SupportedProducts.Any(p => p.Equals(product))).Any(check => check));
        }
    }
}
