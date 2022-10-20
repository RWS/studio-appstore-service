using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.V2
{
    public class VersionProvider<T> : IVersionProvider where T : PluginDetails<PluginVersion<string>, string>, new()
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IAzureRepositoryExtended<T> _azureRepositoryExtended;
        private readonly ILocalRepositoryExtended<T> _localRepositoryExtended;

        public VersionProvider(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _azureRepositoryExtended = new AzureRepositoryExtended<T>(configurationSettings);
            _localRepositoryExtended = new LocalRepositoryExtended<T>(configurationSettings);
        }

        public async Task<string> GetAPIVersion()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepositoryExtended.GetAPIVersionFromContainer();
            }

            return await _localRepositoryExtended.GetAPIVersionFromFile();
        }
    }
}
