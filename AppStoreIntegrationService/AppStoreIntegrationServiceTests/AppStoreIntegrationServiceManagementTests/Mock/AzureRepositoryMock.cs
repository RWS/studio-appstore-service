using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock
{
    public class AzureRepositoryMock : IResponseManager, ISettingsManager, INotificationsManager
    {
        private string _data;
        private string _settings;
        private string _notifications;

        public AzureRepositoryMock() { }

        public AzureRepositoryMock(PluginResponse<PluginDetails> data)
        {
            if (data == null)
            {
                _data = null;
            }

            _data = JsonConvert.SerializeObject(data);
        }

        public AzureRepositoryMock(SiteSettings settings)
        {
            _settings = JsonConvert.SerializeObject(settings);
        }

        public AzureRepositoryMock(IDictionary<string, IEnumerable<PushNotification>> notifications)
        {
            _notifications = JsonConvert.SerializeObject(notifications);
        }

        public async Task<PluginResponseBase<PluginDetails>> GetBaseResponse()
        {
            return await GetResponse();
        }

        public async Task<IDictionary<string, IEnumerable<PushNotification>>> GetNotifications()
        {
            if (string.IsNullOrEmpty(_notifications))
            {
                return new Dictionary<string, IEnumerable<PushNotification>>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<PushNotification>>>(_notifications) ?? new Dictionary<string, IEnumerable<PushNotification>>();
        }

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (string.IsNullOrEmpty(_data))
            {
                return new PluginResponse<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(_data) ?? new PluginResponse<PluginDetails>();
        }

        public async Task<SiteSettings> ReadSettings()
        {
            if (_settings == null)
            {
                return new SiteSettings();
            }

            return JsonConvert.DeserializeObject<SiteSettings>(_settings) ?? new SiteSettings();
        }

        public async Task SaveNotifications(IDictionary<string, IEnumerable<PushNotification>> notifications)
        {
            _notifications = JsonConvert.SerializeObject(notifications);
        }

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            _data = JsonConvert.SerializeObject(response);
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            _settings = JsonConvert.SerializeObject(settings);
        }
    }
}
