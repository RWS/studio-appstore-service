using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly IWritableOptions<SiteSettings> _options;
        private readonly IAzureRepository _azureRepository;
        private readonly IConfigurationSettings _configurationSettings;

        public SettingsRepository(IWritableOptions<SiteSettings> options, IAzureRepository azureRepository, IConfigurationSettings configurationSettings)
        {
            _options = options;
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
        }

        public async Task<SiteSettings> GetSettings()
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                return await _azureRepository.GetSettingsFromContainer();
            }

            return new SiteSettings { Name = _options.Value.Name };
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            if (_configurationSettings.DeployMode == DeployMode.AzureBlob)
            {
                await _azureRepository.UpdateSettingsFileBlob(JsonConvert.SerializeObject(settings));
                return;
            }

            _options.SaveOption(settings);
            _options.Value.Name = settings.Name;
        }
    }
}
