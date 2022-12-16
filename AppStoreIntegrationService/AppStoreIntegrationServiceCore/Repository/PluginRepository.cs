using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Text.RegularExpressions;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class PluginRepository<T> : IPluginRepository<T> where T : PluginDetails<PluginVersion<string>, string>, new()
    {
        private readonly IAzureRepository<T> _azureRepository;
        private readonly ILocalRepository<T> _localRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public PluginRepository
        (
            IAzureRepository<T> azureRepository,
            IConfigurationSettings configurationSettings,
            ILocalRepository<T> localRepository
        )
        {
            _azureRepository = azureRepository;
            _localRepository = localRepository;
            _configurationSettings = configurationSettings;
        }

        public async Task UpdatePrivatePlugin(ExtendedPluginDetails<PluginVersion<string>> privatePlugin)
        {
            var pluginsList = await GetPlugins();

            if (pluginsList != null)
            {
                var pluginExists = pluginsList.Where(p => p.Name.Equals(privatePlugin.Name)).Count() > 1;
                if (pluginExists)
                {
                    throw new Exception($"Another plugin with the name {privatePlugin.Name} already exists");
                }

                await BackupFile(pluginsList);

                var pluginToBeUpdated = pluginsList.FirstOrDefault(p => p.Id.Equals(privatePlugin.Id));

                if (pluginToBeUpdated != null)
                {
                    pluginToBeUpdated.Name = privatePlugin.Name;
                    pluginToBeUpdated.Developer = string.IsNullOrEmpty(privatePlugin.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privatePlugin.DeveloperName };
                    pluginToBeUpdated.ChangelogLink = privatePlugin.ChangelogLink;
                    pluginToBeUpdated.SupportUrl = privatePlugin.SupportUrl;
                    pluginToBeUpdated.SupportEmail = privatePlugin.SupportEmail;
                    pluginToBeUpdated.Description = privatePlugin.Description;
                    pluginToBeUpdated.Icon.MediaUrl = privatePlugin.IconUrl;
                    pluginToBeUpdated.PaidFor = privatePlugin.PaidFor;
                    pluginToBeUpdated.Categories = privatePlugin.Categories;
                    pluginToBeUpdated.Versions = privatePlugin.Versions.Cast<PluginVersion<string>>().ToList();
                    pluginToBeUpdated.DownloadUrl = privatePlugin.DownloadUrl;
                    pluginToBeUpdated.Status = privatePlugin.Status;
                }

                await SaveToFile(pluginsList);
            }
        }

        public async Task AddPrivatePlugin(ExtendedPluginDetails<PluginVersion<string>> privatePlugin)
        {
            if (privatePlugin != null)
            {
                var newPlugin = new T
                {
                    Name = privatePlugin.Name.Trim(),
                    Developer = string.IsNullOrEmpty(privatePlugin.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privatePlugin.DeveloperName },
                    Description = privatePlugin.Description,
                    ChangelogLink = privatePlugin.ChangelogLink,
                    SupportEmail = privatePlugin.SupportEmail,
                    SupportUrl = privatePlugin.SupportUrl,
                    PaidFor = privatePlugin.PaidFor,
                    Categories = privatePlugin.Categories,
                    Versions = privatePlugin.Versions.Cast<PluginVersion<string>>().ToList(),
                    DownloadUrl = privatePlugin.DownloadUrl,
                    Id = privatePlugin.Id,
                    Icon = new IconDetails { MediaUrl = privatePlugin.IconUrl },
                    Status = privatePlugin.Status
                };

                var pluginsList = await GetPlugins();

                if (pluginsList is null)
                {
                    pluginsList = new List<T>
                    {
                        newPlugin
                    };
                }
                else
                {
                    var pluginExists = pluginsList.Any(p => p.Name.Equals(privatePlugin.Name));
                    if (!pluginExists)
                    {
                        await BackupFile(pluginsList);

                        var lastPlugin = pluginsList.OrderBy(p => p.Id).ToList().LastOrDefault();
                        if (lastPlugin != null)
                        {
                            newPlugin.Id = lastPlugin.Id++;
                            privatePlugin.Id = newPlugin.Id;
                        }
                        pluginsList.Add(newPlugin);
                    }
                    else
                    {
                        throw new Exception($"Another plugin with the name {privatePlugin.Name} already exists");
                    }
                }
                await SaveToFile(pluginsList);
            }
        }

        public async Task<T> GetPluginById(int id, string developerName = null)
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

        private async Task<List<T>> GetPlugins()
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                return await _localRepository.ReadPluginsFromFile();
            }

            return await _azureRepository.GetPluginsFromContainer();
        }

        private async Task BackupFile(List<T> plugins)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.BackupFile(plugins);
                return;
            }

            await _localRepository.SavePluginsToFile(plugins);
        }

        public async Task SaveToFile(List<T> pluginsList)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdatePluginsFileBlob(pluginsList);
                return;
            }

            await _localRepository.SavePluginsToFile(pluginsList);
        }

        public async Task<List<T>> GetAll(string sortOrder, string developerName = null)
        {
            var plugins = Equals(developerName, null) switch
            {
                true => (await GetPlugins()).Where(p => p.Status != Status.Draft),
                _ => (await GetPlugins()).Where(p => p.Developer.DeveloperName == developerName).ToList(),
            };

            if (!string.IsNullOrEmpty(sortOrder) && !sortOrder.Equals("asc", StringComparison.CurrentCultureIgnoreCase))
            {
                return plugins?.OrderByDescending(p => p.Name).ToList();
            }

            return plugins?.OrderBy(p => p.Name).ToList();
        }

        private static List<T> FilterByStatus(List<T> plugins, Status status)
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

        private static List<T> FilterByQuery(List<T> pluginsList, string query)
        {
            return pluginsList.Where(p => Regex.IsMatch(p.Name, query, RegexOptions.IgnoreCase)).ToList();
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

        private static List<T> FilterByVersion(List<T> pluginsList, string studioVersion, List<ProductDetails> products)
        {
            var plugins = new List<T>();
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

        private static List<T> FilterByCategory(List<T> pluginsList, List<int> categoryIds)
        {
            return categoryIds.SelectMany(c => pluginsList.Where(p => p.Categories.Any(pc => pc.Equals(c)))).ToList();
        }

        private static List<T> ApplySort(List<T> pluginsList, SortType sortType)
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

        public List<T> SearchPlugins(List<T> pluginsList, PluginFilter filter, List<ProductDetails> products)
        {
            pluginsList ??= new List<T>();

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

        private static List<T> FilterByProduct(List<T> plugins, string product)
        {
            return plugins.Where(p => p.Versions
                          .Select(v => v.SupportedProducts
                          .Any(p => p.Equals(product)))
                          .Any(check => check)).ToList();
        }
    }
}
