using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.V2
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ILocalRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _localRepositoryExtended;
        private readonly IAzureRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _azureRepositoryExtended;
        private readonly IConfigurationSettings _configurationSettings;
        private readonly List<CategoryDetails> _defaultCategories;

        public CategoriesRepository
        (
            ILocalRepositoryExtended<PluginDetails<PluginVersion<string>, string>> localRepositoryExtended, 
            IAzureRepositoryExtended<PluginDetails<PluginVersion<string>, string>> azureRepositoryExtended, 
            IConfigurationSettings configurationSettings
        )
        {
            _localRepositoryExtended = localRepositoryExtended;
            _azureRepositoryExtended = azureRepositoryExtended;
            _configurationSettings = configurationSettings;
            _defaultCategories = new List<CategoryDetails>
            {
                new CategoryDetails
                {
                    Name = "Automated translation",
                    Id = "6"
                },
                new CategoryDetails
                {
                    Name = "Translation memory",
                    Id = "3"
                },
                new CategoryDetails
                {
                    Name = "Process automation & management",
                    Id = "5"
                },
                new CategoryDetails
                {
                    Name = "Reference",
                    Id = "7"
                },
                new CategoryDetails
                {
                    Name = "Terminology",
                    Id = "4"
                },
                new CategoryDetails
                {
                    Name = "File filters & converters",
                    Id = "2"
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
                await _localRepositoryExtended.SaveCategoriesToFile(categories);
                return;
            }

            await _azureRepositoryExtended.UpdateCategoriesFileBlob(categories);
        }

        private async Task<List<CategoryDetails>> GetCategoriesFromPossibleLocation()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepositoryExtended.GetCategoriesFromContainer() ?? _defaultCategories;
            }

            return await _localRepositoryExtended.ReadCategoriesFromFile() ?? _defaultCategories;
        }
    }
}
