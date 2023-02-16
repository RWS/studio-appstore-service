using AppStoreIntegrationServiceCore.Model;
using Newtonsoft.Json;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class AzureRepository : IResponseManager, ISettingsManager, INotificationsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _container;
        private readonly BlobClient _pluginsBlob;
        private readonly BlobClient _settingsBlob;
        private readonly BlobClient _notificationsBlob;

        public AzureRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _blobServiceClient = new BlobServiceClient(new Uri($"https://{_configurationSettings.StorageAccountName}.blob.core.windows.net"), new DefaultAzureCredential());
            _container = _blobServiceClient.GetBlobContainerClient(_configurationSettings.BlobName);

            _pluginsBlob = CreateIfNotExists(_container, _configurationSettings.PluginsFileName);
            _settingsBlob = CreateIfNotExists(_container, _configurationSettings.SettingsFileName);
            _notificationsBlob = CreateIfNotExists(_container, _configurationSettings.NotificationsFileName);
        }

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (_configurationSettings.PluginsFileName == null)
            {
                return new PluginResponse<PluginDetails>();
            }

            var content = await _pluginsBlob.DownloadContentAsync();
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(content.Value.Content.ToString()) ?? new PluginResponse<PluginDetails>();
        }

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
            await _pluginsBlob.UploadAsync(content, new BlobUploadOptions());
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

        private static BlobClient CreateIfNotExists(BlobContainerClient container, string fileName)
        {
            var blob = container.GetBlobClient(fileName);

            if (blob.Exists())
            {
                return blob;
            }

            container.UploadBlob(fileName, new MemoryStream());
            return container.GetBlobClient(fileName);
        }

        public async Task<IDictionary<string, IEnumerable<Notification>>> GetNotifications()
        {
            if (string.IsNullOrEmpty(_configurationSettings.NotificationsFileName))
            {
                return new Dictionary<string, IEnumerable<Notification>>();
            }

            var content = await _notificationsBlob.DownloadContentAsync();

            if (content == null)
            {
                return new Dictionary<string, IEnumerable<Notification>>();
            }

            return JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<Notification>>>(content.Value.Content.ToString()) ?? new Dictionary<string, IEnumerable<Notification>>();
        }

        public async Task SaveNotifications(IDictionary<string, IEnumerable<Notification>> notifications)
        {
            var content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(notifications)));
            await _notificationsBlob.UploadAsync(content, new BlobUploadOptions());
        }
    }
}
