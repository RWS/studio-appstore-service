using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Azure;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepository : IResponseManager, ISettingsManager, INotificationsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IWritableOptions<SiteSettings> _options;

        public LocalRepository(IConfigurationSettings configurationSettings, IWritableOptions<SiteSettings> options = null)
        {
            _configurationSettings = configurationSettings;
            _options = options;
        }

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePath))
            {
                return new PluginResponse<PluginDetails>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePath);

            if (content == null)
            {
                return new PluginResponse<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(content) ?? new PluginResponse<PluginDetails>();
        }

        public async Task<SiteSettings> ReadSettings()
        {
            return new SiteSettings { Name = _options.Value.Name };
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            _options.SaveOption(settings);
            _options.Value.Name = settings.Name;
        }

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task<IDictionary<string, IEnumerable<Notification>>> GetNotifications()
        {
            if (string.IsNullOrEmpty(_configurationSettings.NotificationsFilePath))
            {
                return new Dictionary<string, IEnumerable<Notification>>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.NotificationsFilePath);

            if (content == null)
            {
                return new Dictionary<string, IEnumerable<Notification>>();
            }

            return JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<Notification>>>(content) ?? new Dictionary<string, IEnumerable<Notification>>();
        }

        public async Task SaveNotifications(IDictionary<string, IEnumerable<Notification>> notifications)
        {
            await File.WriteAllTextAsync(_configurationSettings.NotificationsFilePath, JsonConvert.SerializeObject(notifications));
        }
    }
}
