﻿using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock
{
    public class AzureRepositoryMock : IResponseManager, ISettingsManager
    {
        private string _data;
        private string _settings;

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

        public async Task<PluginResponseBase<PluginDetails>> GetBaseResponse()
        {
            return await GetResponse();
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
