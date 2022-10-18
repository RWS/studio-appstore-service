using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Repository.V1
{
    public class PluginRepository<T> : PluginRepositoryBase<T>, IPluginRepository<T> where T : PluginDetails<PluginVersion<ProductDetails>>
    {
        public PluginRepository(IAzureRepository<T> azureRepository, IConfigurationSettings configurationSettings) : base(azureRepository, configurationSettings) { }

        private async Task<List<T>> GetPlugins()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                return await GetPluginsListFromLocalFile();
            }

            return await _azureRepository.GetPluginsListFromContainer();
        }

        private async Task<List<T>> GetPluginsListFromLocalFile()
        {
            var pluginsDetails = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePathV1);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(pluginsDetails)?.Value.Cast<T>().ToList();
        }

        private static List<T> FilterByStatus(List<T> searchedPluginList, PluginFilter.StatusValue status)
        {
            return status switch
            {
                PluginFilter.StatusValue.Active => searchedPluginList.Where(x => !x.Inactive).ToList(),
                PluginFilter.StatusValue.Inactive => searchedPluginList.Where(x => x.Inactive).ToList(),
                _ => searchedPluginList
            };
        }

        private static List<T> FilterByQuery(List<T> pluginsList, string query)
        {
            var searchedPluginsResult = new List<T>();
            foreach (var plugin in pluginsList)
            {
                var matchName = Regex.IsMatch(plugin.Name.ToLower(), query.ToLower());
                if (matchName)
                {
                    searchedPluginsResult.Add(plugin);
                }
            }
            return searchedPluginsResult;

        }

        private static List<T> FilterByPrice(List<T> pluginsList, string price)
        {
            var paidFor = false;

            if (!string.IsNullOrEmpty(price))
            {
                if (price.ToLower().Equals("paid"))
                {
                    paidFor = true;
                }
            }

            return pluginsList.Where(p => p.PaidFor.Equals(paidFor)).ToList();
        }

        private static List<T> FilterByVersion(List<T> pluginsList, string studioVersion)
        {
            var plugins = new List<T>();

            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion<ProductDetails>>();

                foreach (var pluginVersion in plugin.Versions)
                {
                    var version = pluginVersion.SupportedProducts?.FirstOrDefault(s =>
                                  s.ProductName.Equals(oldTradosName) ||
                                  s.ProductName.Equals(rebrandedStudioName) ||
                                  s.ProductName.Equals("SDL Trados Studio") ||
                                  s.ProductName.Equals("Trados Studio"));

                    if (version != null)
                    {
                        matchingVersions.Add(pluginVersion);
                        plugin.DownloadUrl = pluginVersion.DownloadUrl;
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

        private static List<T> FilterByCategory(List<T> pluginsList, List<int> categoryIds)
        {
            var searchedPluginsResult = new List<T>();

            foreach (var categoryId in categoryIds)
            {
                foreach (var plugin in pluginsList)
                {
                    if (plugin.Categories != null)
                    {
                        var containsCategory = plugin.Categories.Any(c => c.Id.Equals(categoryId));
                        if (containsCategory)
                        {
                            var pluginExist = searchedPluginsResult.Any(p => p.Id.Equals(plugin.Id));
                            if (!pluginExist)
                            {
                                searchedPluginsResult.Add(plugin);
                            }
                        }
                    }
                }
            }
            return searchedPluginsResult;
        }

        private static List<T> ApplySort(List<T> pluginsList, PluginFilter.SortType sortType)
        {
            return sortType switch
            {
                PluginFilter.SortType.TopRated => pluginsList.OrderByDescending(p => p.RatingSummary?.AverageOverallRating).ThenBy(p => p.Name).ToList(),
                PluginFilter.SortType.DownloadCount => pluginsList.OrderByDescending(p => p.DownloadCount).ThenBy(p => p.Name).ToList(),
                PluginFilter.SortType.ReviewCount => pluginsList.OrderByDescending(p => p.RatingSummary?.RatingsCount).ThenBy(p => p.Name).ToList(),
                PluginFilter.SortType.LastUpdated => pluginsList.OrderByDescending(p => p.ReleaseDate).ThenBy(p => p.Name).ToList(),
                PluginFilter.SortType.NewlyAdded => pluginsList.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Name).ToList(),
                _ => pluginsList,
            };
        }

        public List<T> SearchPlugins(List<T> pluginsList, PluginFilter filter)
        {
            pluginsList ??= new List<T>();

            List<T> searchedPluginList = FilterByStatus(pluginsList, filter.Status);
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
                searchedPluginList = FilterByVersion(searchedPluginList, filter.StudioVersion);
            }

            if (filter?.CategoryId?.Count > 0)
            {
                searchedPluginList = FilterByCategory(searchedPluginList, filter.CategoryId);
            }

            searchedPluginList = ApplySort(searchedPluginList, filter.SortBy);
            return searchedPluginList;
        }

        public async Task<List<T>> GetAll(string sortOrder)
        {
            var pluginsList = await GetPlugins();

            if (!string.IsNullOrEmpty(sortOrder) && !sortOrder.ToLower().Equals("asc"))
            {
                return pluginsList?.OrderByDescending(p => p.Name).ToList();
            }

            return pluginsList?.OrderBy(p => p.Name).ToList();
        }
    }
}
