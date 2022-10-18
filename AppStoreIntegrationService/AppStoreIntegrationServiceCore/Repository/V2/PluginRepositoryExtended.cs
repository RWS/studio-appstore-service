using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Repository.V2
{
    public class PluginRepositoryExtended<T> : PluginRepositoryBase<T>, IPluginRepositoryExtended<T> where T : PluginDetails<PluginVersion<string>>, new()
    {
        private readonly IAzureRepositoryExtended<T> _azureRepositoryExtended;
        private readonly IProductsRepository _productsRepository;

        public PluginRepositoryExtended
        (
            IAzureRepositoryExtended<T> azureRepositoryExtended,
            IProductsRepository productsRepository,
            IConfigurationSettings configurationSettings,
            HttpClient client
        ) : base(azureRepositoryExtended, configurationSettings, client)
        {
            _azureRepositoryExtended = azureRepositoryExtended;
            _productsRepository = productsRepository;
        }

        public async Task UpdatePrivatePlugin(PrivatePlugin<PluginVersion<string>> privatePlugin)
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
                    pluginToBeUpdated.Inactive = privatePlugin.Inactive;
                }

                await SaveToFile(pluginsList);
            }
        }

        public async Task AddPrivatePlugin(PrivatePlugin<PluginVersion<string>> privatePlugin)
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
                    Inactive = privatePlugin.Inactive
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

        public async Task<T> GetPluginById(int id)
        {
            var pluginList = await GetAll("asc");
            if (pluginList != null)
            {
                return pluginList.FirstOrDefault(p => p.Id.Equals(id));
            }

            return new T();
        }

        public async Task<bool> TryImportPluginsFromFile(IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<PluginResponse<T>>(result.ToString());
                    await SaveToFile(response.Value);
                    return true;

                }
                catch (JsonException)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task RemovePluginVersion(int pluginId, string versionId)
        {
            var pluginList = await GetPlugins();
            var pluginToBeUpdated = pluginList.FirstOrDefault(plugin => plugin.Id.Equals(pluginId));
            var versionToBeRemoved = pluginToBeUpdated.Versions.FirstOrDefault(version => version.Id.Equals(versionId));
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
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                return await GetPluginsListFromLocalFile();
            }

            return await _azureRepository.GetPluginsListFromContainer();
        }

        private async Task<List<T>> GetPluginsListFromLocalFile()
        {
            var pluginsDetails = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePathV2);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(pluginsDetails)?.Value;
        }

        private async Task BackupFile(List<T> pluginsList)
        {
            string updatedPluginsText = JsonConvert.SerializeObject(new PluginResponse<T> { Value = pluginsList });

            if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
            {
                await _azureRepositoryExtended.BackupFile(updatedPluginsText);
            }
            else
            {
                await File.WriteAllTextAsync(_configurationSettings.PluginsFileBackUpPathV2, updatedPluginsText);
            }
        }

        public async Task SaveToFile(List<T> pluginsList)
        {
            string updatedPluginsText = JsonConvert.SerializeObject(new PluginResponse<T> { Value = pluginsList });

            if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
            {
                await _azureRepositoryExtended.UpdatePluginsFileBlob(updatedPluginsText);
            }
            else
            {
                await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, updatedPluginsText);
            }
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

        private List<T> FilterByVersion(List<T> pluginsList, string studioVersion)
        {
            var plugins = new List<T>();
            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion<string>>();

                foreach (var pluginVersion in plugin.Versions)
                {
                    var products = _productsRepository.GetAllProducts().Result.Where(p => p.Id == pluginVersion.SupportedProducts[0]);
                    var version = products.FirstOrDefault(s =>
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
            if (pluginsList is null)
            {
                pluginsList = new List<T>();
            }

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
    }
}
