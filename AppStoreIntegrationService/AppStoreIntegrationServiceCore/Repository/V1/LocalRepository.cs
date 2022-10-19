using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository.V1
{
    public class LocalRepository<T> : ILocalRepository<T> where T : PluginDetails<PluginVersion<ProductDetails>>, new()
    {
        private readonly IConfigurationSettings _configurationSettings;

        public LocalRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        public async Task<List<T>> ReadPluginsFromFile()
        {
            return (await ReadFromFile()).Value;
        }

        private async Task<PluginResponse<T>> ReadFromFile()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePathV1))
            {
                return new PluginResponse<T>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePathV1);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(content);
        }
    }
}
