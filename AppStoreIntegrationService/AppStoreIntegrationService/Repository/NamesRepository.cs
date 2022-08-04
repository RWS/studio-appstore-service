using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppStoreIntegrationService.Model;
using Newtonsoft.Json;

namespace AppStoreIntegrationService.Repository
{
    public class NamesRepository : INamesRepository
    {
        private readonly IAzureRepository _azureRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public NamesRepository(IAzureRepository azureRepository, IConfigurationSettings configurationSettings)
        {
            _azureRepository = azureRepository;
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
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                if (string.IsNullOrEmpty(_configurationSettings.NameMappingsFilePath))
                {
                    return new List<NameMapping>();
                }

                return await ReadLocalNameMappings(_configurationSettings.NameMappingsFilePath);
            }

            return await _azureRepository.GetNameMappingsFromContainer();
        }

        public async Task<List<NameMapping>> ReadLocalNameMappings(string nameMappingsFilePath)
        {
            var nameMappings = await File.ReadAllTextAsync(nameMappingsFilePath);
            return JsonConvert.DeserializeObject<List<NameMapping>>(nameMappings);
        }

        public async Task UpdateNamesMapping(List<NameMapping> namesMapping)
        {
            if (_configurationSettings.DeployMode != Enums.DeployMode.AzureBlob)
            {
                await File.WriteAllTextAsync(_configurationSettings.NameMappingsFilePath, JsonConvert.SerializeObject(namesMapping));
                return;
            }

            await _azureRepository.UpdateNameMappingsFileBlob(JsonConvert.SerializeObject(namesMapping));
        }

        public async Task DeleteNameMapping(string id)
        {
            var newNamesMapping = (await GetNameMappingsFromPossibleLocation()).Where(item => item.Id != id).ToList();
            await UpdateNamesMapping(newNamesMapping);
        }
    }
}
