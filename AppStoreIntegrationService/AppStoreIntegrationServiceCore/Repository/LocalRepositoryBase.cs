using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepositoryBase : IResponseManager
    {
        protected readonly IConfigurationSettings _configurationSettings;

        public LocalRepositoryBase(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        public async Task<PluginResponseBase<PluginDetails>> GetBaseResponse()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePath))
            {
                return new PluginResponseBase<PluginDetails>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePath);

            if (content == null)
            {
                return new PluginResponseBase<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponseBase<PluginDetails>>(content) ?? new PluginResponseBase<PluginDetails>();
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

        public async Task SaveResponse(PluginResponse<PluginDetails> response)
        {
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }
    }
}
