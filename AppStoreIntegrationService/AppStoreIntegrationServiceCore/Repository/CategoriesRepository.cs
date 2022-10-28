using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ILocalRepository<PluginDetails<PluginVersion<string>, string>> _localRepository;
        private readonly IAzureRepository<PluginDetails<PluginVersion<string>, string>> _azureRepository;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly List<CategoryDetails> _defaultCategories;

        public CategoriesRepository
        (
            ILocalRepository<PluginDetails<PluginVersion<string>, string>> localRepository,
            IAzureRepository<PluginDetails<PluginVersion<string>, string>> azureRepository,
            IConfigurationSettings configurationSettings
        )
        {
            _localRepository = localRepository;
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
            _defaultCategories = new List<CategoryDetails>
            {
                new CategoryDetails
                {
                    Name = "File filters & converters",
                    Id = "2"
                },
                new CategoryDetails
                {
                    Name = "Translation memory",
                    Id = "3"
                },
                new CategoryDetails
                {
                    Name = "Terminology",
                    Id = "4"
                },
                new CategoryDetails
                {
                    Name = "Process automation & management",
                    Id = "5"
                },
                new CategoryDetails
                {
                    Name = "Automated translation",
                    Id = "6"
                },
                new CategoryDetails
                {
                    Name = "Reference",
                    Id = "7"
                }
            };
        }

        public async Task DeleteCategory(string id)
        {
            var categories = await GetCategoriesFromPossibleLocation();
            await UpdateCategories(categories.Where(item => item.Id != id).ToList());
        }

        public async Task<List<CategoryDetails>> GetAllCategories()
        {
            return await GetCategoriesFromPossibleLocation();
        }

        public async Task UpdateCategories(List<CategoryDetails> categories)
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                await _localRepository.SaveCategoriesToFile(categories);
                return;
            }

            await _azureRepository.UpdateCategoriesFileBlob(categories);
        }

        private async Task<List<CategoryDetails>> GetCategoriesFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepository.GetCategoriesFromContainer() ?? _defaultCategories;
            }

            return await _localRepository.ReadCategoriesFromFile() ?? _defaultCategories;
        }
    }
}
