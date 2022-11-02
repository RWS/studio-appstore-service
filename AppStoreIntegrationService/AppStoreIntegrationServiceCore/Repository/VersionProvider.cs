using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class VersionProvider<T> : IVersionProvider where T : PluginDetails<PluginVersion<string>, string>, new()
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IAzureRepository<T> _azureRepository;
        private readonly ILocalRepository<T> _localRepository;

        public VersionProvider(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _azureRepository = new AzureRepository<T>(configurationSettings);
            _localRepository = new LocalRepository<T>(configurationSettings);
        }

        public async Task<string> GetAPIVersion()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepository.GetAPIVersionFromContainer();
            }

            return await _localRepository.GetAPIVersionFromFile();
        }

        public async Task UpdateAPIVersion(string version)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdateAPIVersion(version);
                return;
            }

            await _localRepository.SaveAPIVersionToFile(version);
        }
    }
}
