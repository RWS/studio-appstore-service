using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class ResponseRepository : IResponseRepository
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IAzureRepository _azureRepository;
        private readonly ILocalRepository _localRepository;

        public ResponseRepository
        (
            IConfigurationSettings configurationSettings, 
            IAzureRepository azureRepository, 
            ILocalRepository localRepository
        )
        {
            _configurationSettings = configurationSettings;
            _azureRepository = azureRepository;
            _localRepository = localRepository;
        }

        public async Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> GetResponse()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepository.ReadFromContainer();
            }

            return await _localRepository.ReadFromFile();
        }
    }
}
