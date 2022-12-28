using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
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

        public async Task UpdatePlugin(PluginDetails<PluginVersion<string>, string> plugin)
        {
            var pluginsList = await _pluginManager.GetPlugins();

            if (pluginsList == null)
            {
                return;
            }

            var pluginExists = pluginsList.Where(p => p.Name == plugin.Name).Count() > 1;
            if (pluginExists)
            {
                throw new Exception($"Another plugin with the name {plugin.Name} already exists");
            }

            await _pluginManager.BackupPlugins(pluginsList);
            var old = pluginsList.FirstOrDefault(p => p.Id == plugin.Id);

            if (plugin.Versions.Count == 0)
            {
                plugin.Versions = old.Versions;
            }
            
            pluginsList[pluginsList.IndexOf(old)] = plugin;
            await _pluginManager.SavePlugins(pluginsList);
        }

        public async Task AddPlugin(PluginDetails<PluginVersion<string>, string> plugin)
        {
            var plugins = await _pluginManager.GetPlugins();
            if (plugins is null)
            {
                plugins = new List<PluginDetails<PluginVersion<string>, string>> { plugin };
            }
            else
            {
                if (!plugins.Any(p => p.Name == plugin.Name))
                {
                    await _pluginManager.BackupPlugins(plugins);
                    plugins.Add(plugin);
                }
                else
                {
                    throw new Exception($"Another plugin with the name {plugin.Name} already exists");
                }
            }
            await _pluginManager.SavePlugins(plugins);
        }

        public async Task<PluginDetails<PluginVersion<string>, string>> GetPluginById(int id, string developerName = null)
        {
            var pluginList = await GetAll("asc", developerName);

            if (developerName == null)
            {
                return pluginList?.FirstOrDefault(p => p.Id == id);
            }

            return pluginList?.FirstOrDefault(p => p.Id == id && p.Developer.DeveloperName == developerName);
        }

        public async Task RemovePluginVersion(int pluginId, string versionId)
        {
            var pluginList = await _pluginManager.GetPlugins();
            var pluginToBeUpdated = pluginList.FirstOrDefault(plugin => plugin.Id.Equals(pluginId));
            var versionToBeRemoved = pluginToBeUpdated.Versions.FirstOrDefault(version => version.VersionId.Equals(versionId));
            await _pluginManager.BackupPlugins(pluginList);
            pluginToBeUpdated.Versions.Remove(versionToBeRemoved);
            await _pluginManager.SavePlugins(pluginList);
        }

        public async Task UpdatePluginVersion(int pluginId, PluginVersion<string> version)
        {
            var plugin = await GetPluginById(pluginId);
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
            await UpdatePlugin(plugin);
        }


        public async Task RemovePlugin(int id)
        {
            var plugins = await _pluginManager.GetPlugins();
            var plugin = plugins.FirstOrDefault(p => p.Id.Equals(id));
            if (plugin != null)
            {
                await _pluginManager.BackupPlugins(plugins);
                plugins.Remove(plugin);
                await _pluginManager.SavePlugins(plugins);
            }
        }

        public async Task<List<PluginDetails<PluginVersion<string>, string>>> GetAll(string sortOrder, string developerName = null)
        {
            var plugins = Equals(developerName, null) switch
            {
                true => (await _pluginManager.GetPlugins())?.Where(p => p.Status != Status.Draft),
                _ => (await _pluginManager.GetPlugins())?.Where(p => p.Developer.DeveloperName == developerName).ToList(),
            };

            if (!string.IsNullOrEmpty(sortOrder) && !sortOrder.Equals("asc", StringComparison.CurrentCultureIgnoreCase))
            {
                return plugins?.OrderByDescending(p => p.Name).ToList();
            }

            return plugins?.OrderBy(p => p.Name).ToList();
        }

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByStatus(List<PluginDetails<PluginVersion<string>, string>> plugins, Status status)
        {
            return status switch
            {
                Status.Active => plugins.Where(x => x.Status == Status.Active).ToList(),
                Status.Inactive => plugins.Where(x => x.Status == Status.Inactive).ToList(),
                Status.Draft => plugins.Where(x => x.Status == Status.Draft).ToList(),
                Status.InReview => plugins.Where(x => x.Status == Status.InReview).ToList(),
                _ => plugins
            };
        }

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByQuery(List<PluginDetails<PluginVersion<string>, string>> pluginsList, string query)
        {
            return pluginsList.Where(p => Regex.IsMatch(p.Name, query, RegexOptions.IgnoreCase)).ToList();
        }

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByPrice(List<PluginDetails<PluginVersion<string>, string>> pluginsList, string price)
        {
            return pluginsList.Where(p => p.PaidFor == (!string.IsNullOrEmpty(price) && price.Equals("paid", StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByVersion(List<PluginDetails<PluginVersion<string>, string>> pluginsList, string studioVersion, List<ProductDetails> products)
        {
            var plugins = new List<PluginDetails<PluginVersion<string>, string>>();
            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion<string>>();

                foreach (var version in plugin.Versions)
                {
                    var product = version.SupportedProducts.SelectMany(sp => products
                                                           .Where(p => p.Id == sp))
                                                           .FirstOrDefault(s => s.ProductName.Equals(oldTradosName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName + "(CU5)") ||
                                                                                s.ProductName.Equals(rebrandedStudioName + "(CU4)") ||
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

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByCategory(List<PluginDetails<PluginVersion<string>, string>> pluginsList, List<int> categoryIds)
        {
            return categoryIds.SelectMany(c => pluginsList.Where(p => p.Categories.Any(pc => pc.Equals(c)))).ToList();
        }

        private static List<PluginDetails<PluginVersion<string>, string>> ApplySort(List<PluginDetails<PluginVersion<string>, string>> pluginsList, SortType sortType)
        {
            return sortType switch
            {
                SortType.TopRated => pluginsList.OrderByDescending(p => p.RatingSummary?.AverageOverallRating).ThenBy(p => p.Name).ToList(),
                SortType.DownloadCount => pluginsList.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Name).ToList(),
                SortType.ReviewCount => pluginsList.OrderByDescending(p => p.RatingSummary?.RatingsCount).ThenBy(p => p.Name).ToList(),
                SortType.LastUpdated => pluginsList.OrderByDescending(p => p.ReleaseDate).ThenBy(p => p.Name).ToList(),
                SortType.NewlyAdded => pluginsList.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Name).ToList(),
                _ => pluginsList,
            };
        }

        public List<PluginDetails<PluginVersion<string>, string>> SearchPlugins(List<PluginDetails<PluginVersion<string>, string>> pluginsList, PluginFilter filter, List<ProductDetails> products)
        {
            pluginsList ??= new List<PluginDetails<PluginVersion<string>, string>>();

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

            searchedPluginList = ApplySort(searchedPluginList, filter.SortBy);
            return searchedPluginList;
        }

        private static List<PluginDetails<PluginVersion<string>, string>> FilterByProduct(List<PluginDetails<PluginVersion<string>, string>> plugins, string product)
        {
            return plugins.Where(p => p.Versions.Select(v => v.SupportedProducts.Any(p => p.Equals(product))).Any(check => check)).ToList();
        }
    }
}
