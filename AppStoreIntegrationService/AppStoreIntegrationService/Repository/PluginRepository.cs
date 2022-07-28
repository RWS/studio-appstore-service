using AppStoreIntegrationService.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationService.Repository
{
    public class PluginRepository : IPluginRepository
    {
        private const int RefreshDuration = 10;
        private const int CategoryId_AutomatedTranslation = 6;
        private const int CategoryId_TranslationMemory = 3;
        private const int CategoryId_Terminology = 4;
        private const int CategoryId_FileFiltersConverters = 2;
        private const int CategoryId_Reference = 7;
        private const int CategoryId_ProcessReferenceAndAutomation = 5;
        private const int CategoryId_Miscellaneous = 8;
        private const int CategoryId_ContentManagementConnectors = 20;

        private readonly Timer _pluginsCacheRenewer;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IAzureRepository _azureRepository;
        private List<CategoryDetails> _availableCategories;
        private readonly HttpClient _httpClient;

        public PluginRepository(IAzureRepository azureRepository, IConfigurationSettings configurationSettings, HttpClient httpClient)
        {
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
            _httpClient = httpClient;
            _pluginsCacheRenewer = new Timer(OnCacheExpiredCallback,
                this,
                TimeSpan.FromMinutes(RefreshDuration),
                TimeSpan.FromMilliseconds(-1));

            InitializeCategoryList();
        }

        public async Task<List<PluginDetails>> GetAll(string sortOrder)
        {
            var pluginsList = await GetPlugins();

            if (pluginsList == null || pluginsList?.Count == 0)
            {
                await RefreshCacheList();

                pluginsList = await GetPlugins();
            }

            if (!string.IsNullOrEmpty(sortOrder) && !sortOrder.ToLower().Equals("asc"))
            {
                return pluginsList?.OrderByDescending(p => p.Name).ToList();
            }
            return pluginsList?.OrderBy(p => p.Name).ToList();
        }

        private async Task<List<PluginDetails>> GetPlugins()
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob) //json file is saved locally on server
            {
                return await GetPluginsListFromLocalFile(); // read plugins from json file
            }

            return await _azureRepository.GetPluginsListFromContainer(); // json file is on Azure Blob
        }

        private async Task<List<PluginDetails>> GetPluginsListFromLocalFile()
        {
            var pluginsDetails = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsConfigFilePath);

            return JsonConvert.DeserializeObject<PluginsResponse>(pluginsDetails)?.Value;
        }

        private async Task RefreshCacheList()
        {
            if (!string.IsNullOrEmpty(_configurationSettings.OosUri) && _configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_configurationSettings.OosUri}/Apps?$expand=Categories,Versions($expand=SupportedProducts)")
                };
                var pluginsResponse = await _httpClient.SendAsync(httpRequestMessage);
                if (pluginsResponse.IsSuccessStatusCode)
                {
                    if (pluginsResponse.Content != null)
                    {
                        var contentStream = await pluginsResponse.Content?.ReadAsStreamAsync();
                        await _azureRepository.UploadToContainer(contentStream);
                    }
                }
            }
        }

        public List<PluginDetails> SearchPlugins(List<PluginDetails> pluginsList, PluginFilter filter)
        {
            if (pluginsList is null)
            {
                pluginsList = new List<PluginDetails>();
            }
            var searchedPluginList = new List<PluginDetails>(pluginsList);

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

        public async Task<PluginDetails> GetPluginById(int id)
        {
            var pluginList = await GetAll("asc");
            if (pluginList != null)
            {
                return pluginList.FirstOrDefault(p => p.Id.Equals(id));
            }
            return new PluginDetails();
        }

        public async Task<List<CategoryDetails>> GetCategories()
        {
            if (string.IsNullOrEmpty(_configurationSettings.OosUri))
            {
                return _availableCategories;
            }

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_configurationSettings.OosUri}/Categories")
            };
            var categoriesResponse = await _httpClient.SendAsync(httpRequestMessage);
            if (!categoriesResponse.IsSuccessStatusCode || categoriesResponse.Content == null)
            {
                return _availableCategories;
            }

            var content = await categoriesResponse.Content?.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<CategoriesResponse>(content)?.Value;

            const int ParentCategoryId = 1;
            var hiddenCategories = new List<int> {
                CategoryId_Miscellaneous,
                CategoryId_ContentManagementConnectors
            };

            return categories.Where(c => c.ParentCategoryID == ParentCategoryId && !hiddenCategories
                             .Any(hc => hc == c.Id)).ToList();
        }

        private async void OnCacheExpiredCallback(object stateInfo)
        {
            try
            {
                await RefreshCacheList();
            }
            finally
            {
                _pluginsCacheRenewer?.Change(TimeSpan.FromMinutes(RefreshDuration), TimeSpan.FromMilliseconds(-1));
            }
        }

        private List<PluginDetails> FilterByCategory(List<PluginDetails> pluginsList, List<int> categoryIds)
        {
            var searchedPluginsResult = new List<PluginDetails>();

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

        private List<PluginDetails> ApplySort(List<PluginDetails> pluginsList, PluginFilter.SortType sortType)
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

        private List<PluginDetails> FilterByQuery(List<PluginDetails> pluginsList, string query)
        {
            var searchedPluginsResult = new List<PluginDetails>();
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

        private List<PluginDetails> FilterByPrice(List<PluginDetails> pluginsList, string price)
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

        private List<PluginDetails> FilterByVersion(List<PluginDetails> pluginsList, string studioVersion)
        {
            var plugins = new List<PluginDetails>();

            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion>();

				foreach (var pluginVersion in plugin.Versions)
				{
					//there are some apps in the oos which are working for all studio version. So the field is "SDL Trados Studio" without any studio specific version
					var version = pluginVersion.SupportedProducts?.FirstOrDefault(s =>
						s.ProductName.Equals(oldTradosName) || s.ProductName.Equals(rebrandedStudioName)
						                                    || s.ProductName.Equals("SDL Trados Studio") ||
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

        private void InitializeCategoryList()
        {
            _availableCategories = new List<CategoryDetails>
            {
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryAutomatedTranslation,
                    Id = CategoryId_AutomatedTranslation
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryTranslationMemory,
                    Id = CategoryId_TranslationMemory
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryProcessAutomationAndManagement,
                    Id = CategoryId_ProcessReferenceAndAutomation
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryReference,
                    Id = CategoryId_Reference
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryTerminology,
                    Id = CategoryId_Terminology
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryFileFiltersConverters,
                    Id = CategoryId_FileFiltersConverters
                }
            };
        }

        public async Task UpdatePrivatePlugin(PrivatePlugin privatePlugin)
        {
            var pluginsList = await GetPlugins();
            if (pluginsList != null)
            {
                await BackupFile(pluginsList);

                var pluginToBeUpdated = pluginsList.FirstOrDefault(p => p.Id.Equals(privatePlugin.Id));

				if (pluginToBeUpdated != null)
				{
					pluginToBeUpdated.Name = privatePlugin.Name;
					pluginToBeUpdated.Developer = string.IsNullOrEmpty(privatePlugin.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privatePlugin.DeveloperName };
					pluginToBeUpdated.Description = privatePlugin.Description;
					pluginToBeUpdated.Icon.MediaUrl = privatePlugin.IconUrl;
					pluginToBeUpdated.PaidFor = privatePlugin.PaidFor;
					pluginToBeUpdated.Categories = privatePlugin.Categories;
					pluginToBeUpdated.Versions = privatePlugin.Versions;
					pluginToBeUpdated.DownloadUrl = privatePlugin.DownloadUrl;
                    pluginToBeUpdated.Inactive = privatePlugin.Inactive;
				}
				await SaveToFile(pluginsList);
			}
		}

		public async Task AddPrivatePlugin(PrivatePlugin privatePlugin)
		{
			if (privatePlugin != null)
			{
				var newPlugin = new PluginDetails
				{
					Name = privatePlugin.Name,
					Developer = string.IsNullOrEmpty(privatePlugin.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privatePlugin.DeveloperName },
					Description = privatePlugin.Description,
					PaidFor = privatePlugin.PaidFor,
					Categories = privatePlugin.Categories,
					Versions = privatePlugin.Versions,
					DownloadUrl = privatePlugin.DownloadUrl,
					Id = privatePlugin.Id,
					Icon = new IconDetails { MediaUrl = privatePlugin.IconUrl },
                    Inactive = privatePlugin.Inactive
				};

                var pluginsList = await GetPlugins();

                if (pluginsList is null)
                {
                    pluginsList = new List<PluginDetails>
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

        private async Task BackupFile(List<PluginDetails> pluginsList)
        {
            var pluginResponse = new PluginsResponse
            {
                Value = pluginsList
            };

            var updatedPluginsText = JsonConvert.SerializeObject(pluginResponse);
            if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
            {
                await _azureRepository.BackupFile(updatedPluginsText);
            }
            else
            {
                //json file is saved locally on server or in File Newtork location
                await File.WriteAllTextAsync(_configurationSettings.ConfigFileBackUpPath, updatedPluginsText);
            }
        }

        private async Task SaveToFile(List<PluginDetails> pluginsList)
        {
            var pluginResponse = new PluginsResponse
            {
                Value = pluginsList
            };
            var updatedPluginsText = JsonConvert.SerializeObject(pluginResponse);

            if (_configurationSettings.DeployMode == Enums.DeployMode.AzureBlob)
            {
                await _azureRepository.UpdatePluginsFileBlob(updatedPluginsText);
            }
            else
            {
                //json file is saved locally on server or in File Newtork location
                await File.WriteAllTextAsync(_configurationSettings.LocalPluginsConfigFilePath, updatedPluginsText);
            }
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

        public async Task RemovePluginVersion(int pluginId, string versionId)
        {
            var pluginList = await GetPlugins();
            var pluginToBeUpdated = pluginList.FirstOrDefault(plugin => plugin.Id.Equals(pluginId));
            var versionToBeRemoved = pluginToBeUpdated.Versions.FirstOrDefault(version => version.Id.Equals(versionId));
            await BackupFile(pluginList);
            pluginToBeUpdated.Versions.Remove(versionToBeRemoved);
            await SaveToFile(pluginList);
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
                    var response = JsonConvert.DeserializeObject<PluginsResponse>(result.ToString());
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
    }
}
