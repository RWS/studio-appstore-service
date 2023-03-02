using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepositoryBase : IResponseManagerBase
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
    }
}
