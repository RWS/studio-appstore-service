﻿using AppStoreIntegrationServiceCore.Model;
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
    public class AzureRepository : AzureRepositoryBase, IResponseManager, ISettingsManager, INotificationsManager
    {
        private readonly BlobClient _settingsBlob;
        private readonly BlobClient _notificationsBlob;

        public AzureRepository(IConfigurationSettings configurationSettings) : base(configurationSettings)
        {
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