using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Text.RegularExpressions;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class PluginRepository : IPluginRepository
    {
        private readonly IAzureRepository _azureRepository;
        private readonly ILocalRepository _localRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public PluginRepository
        (
            IAzureRepository azureRepository,
            IConfigurationSettings configurationSettings,
            ILocalRepository localRepository
        )
        {
            _azureRepository = azureRepository;
            _localRepository = localRepository;
            _configurationSettings = configurationSettings;
        }

        public async Task UpdatePlugin(PluginDetails<PluginVersion<string>, string> plugin)
        {
            var pluginsList = await GetPlugins();

            if (pluginsList == null)
            {
                return;
            }

            var pluginExists = pluginsList.Where(p => p.Name == plugin.Name).Count() > 1;
            if (pluginExists)
            {
                throw new Exception($"Another plugin with the name {plugin.Name} already exists");
            }

            await BackupFile(pluginsList);
            pluginsList[pluginsList.IndexOf(pluginsList.FirstOrDefault(p => p.Id == plugin.Id))] = plugin;
            await SaveToFile(pluginsList);
        }

        public async Task AddPlugin(PluginDetails<PluginVersion<string>, string> plugin)
        {
            if (plugin != null)
            {
                var pluginsList = await GetPlugins();

                if (pluginsList is null)
                {
                    pluginsList = new List<PluginDetails<PluginVersion<string>, string>> { plugin };
                }
                else
                {
                    var pluginExists = pluginsList.Any(p => p.Name == plugin.Name);
                    if (!pluginExists)
                    {
                        await BackupFile(pluginsList);

                        var lastPlugin = pluginsList.OrderBy(p => p.Id).ToList().LastOrDefault();
                        if (lastPlugin != null)
                        {
                            plugin.Id = lastPlugin.Id++;
                            plugin.Id = plugin.Id;
                        }
                        pluginsList.Add(plugin);
                    }
                    else
                    {
                        throw new Exception($"Another plugin with the name {plugin.Name} already exists");
                    }
                }
                await SaveToFile(pluginsList);
            }
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
            var pluginList = await GetPlugins();
            var pluginToBeUpdated = pluginList.FirstOrDefault(plugin => plugin.Id.Equals(pluginId));
            var versionToBeRemoved = pluginToBeUpdated.Versions.FirstOrDefault(version => version.VersionId.Equals(versionId));
            await BackupFile(pluginList);
            pluginToBeUpdated.Versions.Remove(versionToBeRemoved);
            await SaveToFile(pluginList);
        }

        public async Task RemovePlugin(int id)
        {
            var pluginsList = await GetPlugins();
            var pluginToBeDeleted = pluginsList.FirstOrDefault(p => p.Id.Equals(id));
            if (pluginToBeDeleted != null)
            {
                await BackupFile(pluginsList);
                pluginsList.Remove(pluginToBeDeleted);
                await SaveToFile(pluginsList);
            }
        }

        private async Task<List<PluginDetails<PluginVersion<string>, string>>> GetPlugins()
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return await _localRepository.ReadPluginsFromFile();
            }

            return await _azureRepository.GetPluginsFromContainer();
        }

        private async Task BackupFile(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.BackupFile(plugins);
                return;
            }

            await _localRepository.SavePluginsToFile(plugins);
        }

        public async Task SaveToFile(List<PluginDetails<PluginVersion<string>, string>> pluginsList)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdatePluginsFileBlob(pluginsList);
                return;
            }

            await _localRepository.SavePluginsToFile(pluginsList);
        }

        public async Task<List<PluginDetails<PluginVersion<string>, string>>> GetAll(string sortOrder, string developerName = null)
        {
            var plugins = Equals(developerName, null) switch
            {
                true => (await GetPlugins())?.Where(p => p.Status != Status.Draft),
                _ => (await GetPlugins())?.Where(p => p.Developer.DeveloperName == developerName).ToList(),
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
                                                                                s.ProductName.Equals("SDL Trados Studio") ||
                                                                                s.ProductName.Equals("Trados Studio"));

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
