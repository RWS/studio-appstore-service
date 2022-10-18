using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Newtonsoft.Json;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IWritableOptions<SiteSettings> _options;
        private readonly IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> _azureRepositoryExtended;
        private readonly IConfigurationSettings _configurationSettings;

        public SettingsRepository
        (
            IWritableOptions<SiteSettings> options, 
            IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>> azureRepositoryExtended, 
            IConfigurationSettings configurationSettings
        )
        {
            _options = options;
            _azureRepositoryExtended = azureRepositoryExtended;
            _configurationSettings = configurationSettings;
        }

        public async Task<SiteSettings> GetSettings()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepositoryExtended.GetSettingsFromContainer();
            }

            return new SiteSettings { Name = _options.Value.Name };
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepositoryExtended.UpdateSettingsFileBlob(JsonConvert.SerializeObject(settings));
                return;
            }

            _options.SaveOption(settings);
            _options.Value.Name = settings.Name;
        }
    }
}
