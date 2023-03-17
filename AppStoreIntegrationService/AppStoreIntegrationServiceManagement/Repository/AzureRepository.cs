using AppStoreIntegrationServiceCore.Model;
using Newtonsoft.Json;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using AppStoreIntegrationServiceAPI.Model.Repository;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Settings;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class AzureRepository : AzureRepositoryBase, ISettingsManager, INotificationsManager
    {
        private readonly BlobClient _settingsBlob;
        private readonly BlobClient _notificationsBlob;

        public AzureRepository(IConfigurationSettings configurationSettings) : base(configurationSettings)
        {
            _settingsBlob = CreateIfNotExists(_container, _configurationSettings.SettingsFileName);
            _notificationsBlob = CreateIfNotExists(_container, _configurationSettings.NotificationsFileName);
        }

        public async Task<SiteSettings> ReadSettings()
        {
            if (_configurationSettings.SettingsFileName == null)
            {
                return new SiteSettings();
            }

            var content = await _settingsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<SiteSettings>(content.Value.Content.ToString()) ?? new SiteSettings();
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings)));
            await _settingsBlob.UploadAsync(content, new BlobUploadOptions());
        }

        public async Task<IDictionary<string, IEnumerable<PushNotification>>> GetNotifications()
        {
            if (string.IsNullOrEmpty(_configurationSettings.NotificationsFileName))
            {
                return new Dictionary<string, IEnumerable<PushNotification>>();
            }

            var content = await _notificationsBlob.DownloadContentAsync();

            if (content == null)
            {
                return new Dictionary<string, IEnumerable<PushNotification>>();
            }

            return JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<PushNotification>>>(content.Value.Content.ToString()) ?? new Dictionary<string, IEnumerable<PushNotification>>();
        }

        public async Task SaveNotifications(IDictionary<string, IEnumerable<PushNotification>> notifications)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(notifications)));
            await _notificationsBlob.UploadAsync(content, new BlobUploadOptions());
        }
    }
}
