using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class NamesRepository : INamesRepository
    {
        private readonly IAzureRepository<PluginDetails<PluginVersion<string>, string>> _azureRepository;
        private readonly ILocalRepository<PluginDetails<PluginVersion<string>, string>> _localRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public NamesRepository
        (
            IAzureRepository<PluginDetails<PluginVersion<string>, string>> azureRepository,
            ILocalRepository<PluginDetails<PluginVersion<string>, string>> localRepository,
            IConfigurationSettings configurationSettings
        )
        {
            _azureRepository = azureRepository;
            _localRepository = localRepository;
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
                DeployMode.AzureBlob => await _azureRepository.GetNameMappingsFromContainer(),
                _ => await _localRepository.ReadMappingsFromFile()
            };
        }

        public async Task UpdateMappings(List<NameMapping> names)
        {
            if (_configurationSettings.DeployMode != DeployMode.AzureBlob)
            {
                await _localRepository.SaveMappingsToFile(names);
                return;
            }

            await _azureRepository.UpdateMappingsFileBlob(names);
        }

        public async Task DeleteMapping(string id)
        {
            var newNames = (await GetNameMappingsFromPossibleLocation()).Where(item => item.Id != id).ToList();
            await UpdateMappings(newNames);
        }
    }
}
