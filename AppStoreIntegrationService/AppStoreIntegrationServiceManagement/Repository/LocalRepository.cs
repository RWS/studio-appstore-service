using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class LocalRepository : LocalRepositoryBase, ISettingsManager, INotificationsManager
    {
        private readonly IWritableOptions<SiteSettings> _options;

        public LocalRepository(IConfigurationSettings configurationSettings, IWritableOptions<SiteSettings> options) : base(configurationSettings)
        {
            _options = options;
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

        public async Task<IDictionary<string, IEnumerable<PushNotification>>> GetNotifications()
        {
            if (string.IsNullOrEmpty(_configurationSettings.NotificationsFilePath))
            {
                return new Dictionary<string, IEnumerable<PushNotification>>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.NotificationsFilePath);

            if (content == null)
            {
                return new Dictionary<string, IEnumerable<PushNotification>>();
            }

            return JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<PushNotification>>>(content) ?? new Dictionary<string, IEnumerable<PushNotification>>();
        }

        public async Task SaveNotifications(IDictionary<string, IEnumerable<PushNotification>> notifications)
        {
            await File.WriteAllTextAsync(_configurationSettings.NotificationsFilePath, JsonConvert.SerializeObject(notifications));
        }
    }
}
