using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class ResponseRepository<T> : IResponseRepository<T>
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IAzureRepository<T> _azureRepository;
        private readonly ILocalRepository<T> _localRepository;

        public ResponseRepository(IConfigurationSettings configurationSettings, IAzureRepository<T> azureRepository, ILocalRepository<T> localRepository)
        {
            _configurationSettings = configurationSettings;
            _azureRepository = azureRepository;
            _localRepository = localRepository;
        }

        public async Task<PluginResponse<T>> GetResponse()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepository.ReadFromContainer();
            }

            return await _localRepository.ReadFromFile();
        }
    }
}
