using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository.V2
{
    public class LocalRepositoryExtended<T> : ILocalRepositoryExtended<T> where T : PluginDetails<PluginVersion<string>>, new()
    {
        private readonly IConfigurationSettings _configurationSettings;

        public LocalRepositoryExtended(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        private async Task<PluginResponse<T>> ReadFromFile()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePathV2))
            {
                return new PluginResponse<T>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePathV2);
            return JsonConvert.DeserializeObject<PluginResponse<T>>(content);
        }

        public async Task<List<NameMapping>> ReadMappingsFromFile()
        {
            if (_configurationSettings.NameMappingsFilePath == null)
            {
                return new List<NameMapping>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.NameMappingsFilePath);
            return JsonConvert.DeserializeObject<List<NameMapping>>(content) ?? new List<NameMapping>();
        }

        public async Task<List<ParentProduct>> ReadParentsFromFile()
        {
            return (await ReadFromFile()).ParentProducts ?? new List<ParentProduct>();
        }

        public async Task<List<T>> ReadPluginsFromFile()
        {
            return (await ReadFromFile()).Value ?? new List<T>();
        }

        public async Task<List<ProductDetails>> ReadProductsFromFile()
        {
            return (await ReadFromFile()).Products ?? new List<ProductDetails>();
        }

        public async Task SaveMappingsToFile(List<NameMapping> names)
        {
            await File.WriteAllTextAsync(_configurationSettings.NameMappingsFilePath, JsonConvert.SerializeObject(names));
        }

        public async Task SaveParentsToFile(List<ParentProduct> products)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                Value = response.Value,
                Products = response.Products,
                ParentProducts = products
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, text);
        }

        public async Task SavePluginsToFile(List<T> plugins)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                Value = plugins,
                Products = response.Products,
                ParentProducts = response.ParentProducts
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, text);
        }

        public async Task SaveProductsToFile(List<ProductDetails> products)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<T>
            {
                Value = response.Value,
                Products = products,
                ParentProducts = response.ParentProducts
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePathV2, text);
        }
    }
}
