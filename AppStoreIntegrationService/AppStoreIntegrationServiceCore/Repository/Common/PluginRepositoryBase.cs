using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class PluginRepositoryBase<T>
    {
        private const int CategoryId_Miscellaneous = 8;
        private const int CategoryId_ContentManagementConnectors = 20;
        private const int CategoryId_AutomatedTranslation = 6;
        private const int CategoryId_TranslationMemory = 3;
        private const int CategoryId_Terminology = 4;
        private const int CategoryId_FileFiltersConverters = 2;
        private const int CategoryId_Reference = 7;
        private const int CategoryId_ProcessReferenceAndAutomation = 5;
        private const int RefreshDuration = 10;

        protected readonly IConfigurationSettings _configurationSettings;
        protected readonly IAzureRepository<T> _azureRepository;
        private readonly HttpClient _httpClient;
        private List<CategoryDetails> _availableCategories;
        private readonly Timer _pluginsCacheRenewer;

        public PluginRepositoryBase(IAzureRepository<T> azureRepository, IConfigurationSettings configurationSettings, HttpClient httpClient)
        {
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
            _httpClient = httpClient;
            InitializeCategoryList();

            _pluginsCacheRenewer = new Timer(OnCacheExpiredCallback,
                this,
                TimeSpan.FromMinutes(RefreshDuration),
                TimeSpan.FromMilliseconds(-1));
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

            var hiddenCategories = new List<int> {
                CategoryId_Miscellaneous,
                CategoryId_ContentManagementConnectors
            };

            return categories.Where(c => !hiddenCategories.Any(hc => hc == c.Id)).ToList();
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

        protected async Task RefreshCacheList()
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
    }
}
