using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class NamesRepository : INamesRepository
    {
        private readonly IAzureRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _azureRepositoryExtended;
        private readonly ILocalRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _localRepositoryExtended;
        private readonly IConfigurationSettings _configurationSettings;

        public NamesRepository
        (
            IAzureRepositoryExtended<PluginDetails<PluginVersion<string>, string>> azureRepositoryExtended,
            ILocalRepositoryExtended<PluginDetails<PluginVersion<string>, string>> localRepositoryExtended,
            IConfigurationSettings configurationSettings
        )
        {
            _azureRepositoryExtended = azureRepositoryExtended;
            _localRepositoryExtended = localRepositoryExtended;
            _configurationSettings = configurationSettings;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNameMappings()
        {
            return await GetNameMappingsFromPossibleLocation();
        }

        public async Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames)
        {
            var nameMappings = await GetNameMappingsFromPossibleLocation();
            return pluginsNames.Select(pluginName => nameMappings
                               .FirstOrDefault(n => n.OldName.Equals(pluginName)))
                               .Where(mapping => mapping != null);
        }

        private async Task<List<NameMapping>> GetNameMappingsFromPossibleLocation()
        {
            return _configurationSettings.DeployMode switch
            {
                DeployMode.AzureBlob => await _azureRepositoryExtended.GetNameMappingsFromContainer(),
                _ => await _localRepositoryExtended.ReadMappingsFromFile()
            };
        }

        public async Task UpdateMappings(List<NameMapping> names)
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                await _localRepositoryExtended.SaveMappingsToFile(names);
                return;
            }

            await _azureRepositoryExtended.UpdateMappingsFileBlob(names);
        }

        public async Task DeleteMapping(string id)
        {
            var newNames = (await GetNameMappingsFromPossibleLocation()).Where(item => item.Id != id).ToList();
            await UpdateMappings(newNames);
        }
    }
}
